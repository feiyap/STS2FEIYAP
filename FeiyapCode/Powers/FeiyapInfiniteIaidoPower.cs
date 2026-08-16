using System.Collections.Generic;
using System.Threading.Tasks;
using Feiyap.Mechanics;
using Feiyap.Patches;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Characters;
using STS2RitsuLib.Scaffolding.Content;

namespace Feiyap.Powers;

/// <summary>
/// 无限居合：本回合居合视为 ∞，负责全额格挡与反击；不参与数值累计，下回合开始时移除。
/// 减伤在伤害乘算（如易伤）之后的 Cap 阶段结算。
/// </summary>
[RegisterPower]
public sealed class FeiyapInfiniteIaidoPower : ModPowerTemplate
{
    private bool _pendingBlock;
    private decimal _pendingIncomingDamage;
    private CardPlay? _pendingCardPlay;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override PowerAssetProfile AssetProfile =>
        FeiyapPowerAssets.ForSharedIcon(nameof(FeiyapInfiniteIaidoPower), nameof(FeiyapIaidoSurgePower));

    /// <summary>居合 UI 已在血条显示 ∞，能力栏隐藏以免误触发失去动画。</summary>
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

        // 意图预览不得清空真实受击的待结算（荆棘嵌套掉血后可能刷新意图）。
        if (FeiyapIaidoIntentPreviewScope.IsActive)
        {
            return 0m;
        }

        if (amount <= 0m || !FeiyapIaidoCombat.IsBlockableDamage(Owner, dealer, props))
        {
            ClearPending();
            return 0m;
        }

        _pendingBlock = true;
        _pendingIncomingDamage = amount;
        _pendingCardPlay = cardPlay;
        // 加算阶段不减伤，留到乘算之后全额抵消。
        return 0m;
    }

    public override decimal ModifyDamageCap(
        Creature? target,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource,
        CardPlay? cardPlay)
    {
        // 预览阶段即使仍有真实受击 pending，也不得全额抵消意图伤害。
        if (FeiyapIaidoIntentPreviewScope.IsActive || !_pendingBlock || target != Owner)
        {
            return decimal.MaxValue;
        }

        return 0m;
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

        if (!_pendingBlock)
        {
            ClearPending();
            return;
        }

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
        var blockedDamage = Math.Max(0m, damageWithoutIaido - amount);
        if (blockedDamage <= 0m)
        {
            return;
        }

        var isPerfect = FeiyapIaidoCombat.ShouldCounter(Owner, dealer, props)
                        && Owner.FindPower<FeiyapIaidoSurgePower>() != null;

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

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player.Creature != Owner)
        {
            return;
        }

        await PowerCmd.Remove(this);
        IaidoHealthBarOverlay.RefreshForCreature(Owner);
    }

    private void ClearPending()
    {
        _pendingBlock = false;
        _pendingIncomingDamage = 0m;
        _pendingCardPlay = null;
    }
}
