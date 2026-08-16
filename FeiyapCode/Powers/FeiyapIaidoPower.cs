using System.Collections.Generic;
using System.Threading.Tasks;
using Feiyap.Mechanics;
using Feiyap.Patches;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Characters;
using STS2RitsuLib.Scaffolding.Content;

namespace Feiyap.Powers;

/// <summary>
/// 居合：受到攻击时消耗等量居合格挡并反击。
/// 减伤在伤害乘算（如易伤）之后的 Cap 阶段结算，使抵消/反击量与最终受伤一致。
/// </summary>
[RegisterPower]
public sealed class FeiyapIaidoPower : ModPowerTemplate
{
    private bool _pendingBlock;
    private int _pendingConsume;
    private decimal _pendingIncomingDamage;
    private CardPlay? _pendingCardPlay;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => FeiyapPowerAssets.For(nameof(FeiyapIaidoPower));

    /// <summary>居合数值由血条 UI 显示，能力栏隐藏。</summary>
    protected override bool IsVisibleInternal => false;

    protected override IEnumerable<string> RegisteredKeywordIds => [FeiyapKeywords.IaidoId];

    public override decimal ModifyDamageAdditive(
        Creature? target,
        decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource,
        CardPlay? cardPlay)
    {
        // 非自身受伤：不触碰 pending，避免荆棘等嵌套伤害清空待结算状态。
        if (target != Owner)
        {
            return 0m;
        }

        // 意图预览会走 ModifyDamage，但不得清空真实受击的待结算。
        // 荆棘在 BeforeDamageReceived 嵌套掉血后，可能在 await 间隙刷新意图，
        // 若此处 ClearPending，后续居合反击会被吞掉。
        if (FeiyapIaidoIntentPreviewScope.IsActive)
        {
            return 0m;
        }

        // 无限居合由 FeiyapInfiniteIaidoPower 独占处理，避免双重减伤。
        if (FeiyapIaidoCmd.IsInfinite(Owner))
        {
            ClearPending();
            return 0m;
        }

        if (!ShouldConsumeIaido(target, dealer, amount, props))
        {
            ClearPending();
            return 0m;
        }

        _pendingBlock = true;
        _pendingIncomingDamage = amount;
        _pendingCardPlay = cardPlay;
        // 加算阶段不减伤，留到乘算（易伤等）之后的 Cap 阶段再抵消。
        return 0m;
    }

    public override decimal ModifyDamageCap(
        Creature? target,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource,
        CardPlay? cardPlay)
    {
        // 预览阶段即使仍有真实受击 pending，也不得用居合压低意图伤害数字。
        if (FeiyapIaidoIntentPreviewScope.IsActive
            || !_pendingBlock
            || target != Owner
            || Amount <= 0)
        {
            return decimal.MaxValue;
        }

        var fullDamage = FeiyapIaidoCombat.ComputeDamageWithoutPower(
            this,
            Owner,
            _pendingIncomingDamage,
            props,
            dealer,
            cardSource,
            _pendingCardPlay ?? cardPlay);

        var consume = (int)Math.Min(Amount, Math.Round(fullDamage));
        if (consume <= 0)
        {
            ClearPending();
            return decimal.MaxValue;
        }

        _pendingConsume = consume;
        return Math.Max(0m, fullDamage - consume);
    }

    public override async Task BeforeDamageReceived(
        PlayerChoiceContext choiceContext,
        Creature target,
        decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        // 其他单位受伤（如荆棘反伤）不得清空自身待结算的居合。
        if (target != Owner)
        {
            return;
        }

        if (!_pendingBlock || _pendingConsume <= 0)
        {
            ClearPending();
            return;
        }

        var consume = _pendingConsume;
        var preIaidoRunningTotal = _pendingIncomingDamage;
        var pendingCardPlay = _pendingCardPlay;
        ClearPending();

        var damageWithoutIaido = FeiyapIaidoCombat.ComputeDamageWithoutPower(
            this,
            Owner,
            preIaidoRunningTotal,
            props,
            dealer,
            cardSource,
            pendingCardPlay);
        var blockedDamage = Math.Max(0m, Math.Min(consume, damageWithoutIaido - amount));
        var actualConsume = (int)Math.Round(blockedDamage);
        if (actualConsume <= 0)
        {
            return;
        }

        var forcedPerfect = Owner.FindPower<FeiyapIaidoSurgePower>() != null;
        var isPerfect = FeiyapIaidoCombat.ShouldCounter(Owner, dealer, props)
                        && (forcedPerfect || Amount == damageWithoutIaido);

        await PowerCmd.Apply(choiceContext, this, Owner, -actualConsume, Owner, null);

        await FeiyapIaidoCombat.ResolveBlocked(
            choiceContext,
            Owner,
            this,
            blockedDamage,
            props,
            dealer,
            isPerfect);
    }

    public override Task AfterModifyingDamageAmount(CardModel? cardSource)
    {
        if (_pendingBlock)
        {
            Flash();
            IaidoHealthBarOverlay.RefreshForCreature(Owner);
        }

        return Task.CompletedTask;
    }

    public override Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner?.Creature != Owner)
        {
            return Task.CompletedTask;
        }

        // 绯神乐：下一张攻击或技能不消耗居合，并在打出后移除。
        if (cardPlay.Card.Type is CardType.Attack or CardType.Skill
            && Owner.FindPower<FeiyapScarletKaguraPower>() is { } scarlet)
        {
            return PowerCmd.Remove(scarlet);
        }

        if (cardPlay.Card.Type != CardType.Attack)
        {
            return Task.CompletedTask;
        }

        if (FeiyapIaidoCmd.IsInfinite(Owner)
            || FeiyapCardTags.SkipIaidoConsumeOnPlay(cardPlay.Card)
            || Amount <= 0)
        {
            return Task.CompletedTask;
        }

        var reduce = Math.Min(Amount, 2);
        return PowerCmd.Apply(choiceContext, this, Owner, -reduce, Owner, null);
    }

    public override Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player.Creature != Owner)
        {
            return Task.CompletedTask;
        }

        FeiyapCombatTracker.Get(player).OnTurnStart(player);

        var tracker = FeiyapCombatTracker.Get(player);
        if (tracker.RetainIaidoNextTurn)
        {
            tracker.RetainIaidoNextTurn = false;
            return Task.CompletedTask;
        }

        return FeiyapIaidoCmd.ClearAll(choiceContext, Owner);
    }

    private bool ShouldConsumeIaido(Creature? target, Creature? dealer, decimal amount, ValueProp props) =>
        target == Owner
        && amount > 0m
        && Amount > 0
        && FeiyapIaidoCombat.IsBlockableDamage(Owner, dealer, props);

    private void ClearPending()
    {
        _pendingBlock = false;
        _pendingConsume = 0;
        _pendingIncomingDamage = 0m;
        _pendingCardPlay = null;
    }
}
