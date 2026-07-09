using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Feiyap.Mechanics;
using Feiyap.Patches;
using Feiyap.Powers;
using MegaCrit.Sts2.Core.Combat;
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
/// ??????????????????????????????
/// </summary>
[RegisterPower]
public sealed class FeiyapIaidoPower : ModPowerTemplate
{
    private int _pendingConsume;
    private decimal _pendingIncomingDamage;
    private CardPlay? _pendingCardPlay;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => FeiyapPowerAssets.For(nameof(FeiyapIaidoPower));

    /// <summary>???????? UI ???????????</summary>
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
        if (FeiyapIaidoIntentPreviewScope.IsActive)
        {
            ClearPendingConsume();
            return 0m;
        }

        if (!ShouldConsumeIaido(target, dealer, amount, props))
        {
            ClearPendingConsume();
            return 0m;
        }

        var consume = (int)Math.Min(Amount, amount);
        if (consume <= 0)
        {
            ClearPendingConsume();
            return 0m;
        }

        _pendingConsume = consume;
        _pendingIncomingDamage = amount;
        _pendingCardPlay = cardPlay;
        return -consume;
    }

    public override async Task BeforeDamageReceived(
        PlayerChoiceContext choiceContext,
        Creature target,
        decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        if (target != Owner || _pendingConsume <= 0)
        {
            ClearPendingConsume();
            return;
        }

        var consume = _pendingConsume;
        var pendingCardPlay = _pendingCardPlay;
        ClearPendingConsume();
        var shouldCounter = ShouldIaidoCounter(dealer, props);

        var forcedPerfect = shouldCounter && Owner.FindPower<FeiyapIaidoSurgePower>() != null;
        var attackDamage = ComputeAttackDamageWithoutIaido(consume, amount, props, dealer, cardSource, pendingCardPlay);
        var blockedDamage = Math.Max(0m, attackDamage - amount);
        var isPerfect = shouldCounter && (forcedPerfect || Amount == attackDamage);
        var counterDamage = shouldCounter
            ? FeiyapIaidoCmd.ApplyCounterDamageMultiplier(Owner, blockedDamage, isPerfect)
            : 0m;

        Flash();

        await PowerCmd.Apply(choiceContext, this, Owner, -consume, Owner, null);
        IaidoHealthBarOverlay.RefreshForCreature(Owner);

        if (Owner.Player != null)
        {
            FeiyapMusouCmd.OnIaidoBlocked(Owner.Player, (int)Math.Round(blockedDamage));
        }

        if (!shouldCounter)
        {
            return;
        }

        if (isPerfect)
        {
            await FeiyapPerfectIaidoCmd.Trigger(choiceContext, Owner, blockedDamage);
        }

        await FeiyapSwordSaintHeartPower.OnIaidoTriggered(choiceContext, Owner, perfect: isPerfect);

        var heavenMay = Owner.FindPower<FeiyapHeavenMayPower>();
        if (heavenMay != null)
        {
            var enemies = Owner.CombatState?.GetOpponentsOf(Owner).Where(e => e.IsAlive).ToList() ?? [];
            await FeiyapSlashCmd.PlayIaidoCounterSlashAll(enemies, async () =>
            {
                foreach (var enemy in enemies)
                {
                    await CreatureCmd.Damage(
                        choiceContext,
                        enemy,
                        counterDamage,
                        ValueProp.Unpowered | ValueProp.SkipHurtAnim,
                        Owner,
                        null,
                        null);
                }

                if (Owner.Player != null && enemies.Count > 0)
                {
                    FeiyapQuestProgress.RecordIaidoDamage(
                        Owner.Player,
                        (int)Math.Round(counterDamage * enemies.Count));
                }
            });
        }
        else if (dealer != null && dealer.Side != Owner.Side && dealer.IsAlive)
        {
            await FeiyapSlashCmd.PlayIaidoCounterSlash(dealer, async () =>
            {
                await CreatureCmd.Damage(
                    choiceContext,
                    dealer,
                    counterDamage,
                    ValueProp.Unpowered | ValueProp.SkipHurtAnim,
                    Owner,
                    null,
                    null);

                if (Owner.Player != null)
                {
                    FeiyapQuestProgress.RecordIaidoDamage(Owner.Player, (int)Math.Round(counterDamage));
                }
            });
        }
    }

    public override Task AfterModifyingDamageAmount(CardModel? cardSource)
    {
        if (_pendingConsume > 0)
        {
            Flash();
            IaidoHealthBarOverlay.RefreshForCreature(Owner);
        }

        return Task.CompletedTask;
    }

    public override Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner?.Creature != Owner || cardPlay.Card.Type != CardType.Attack)
        {
            return Task.CompletedTask;
        }

        if (FeiyapCardTags.SkipIaidoConsumeOnPlay(cardPlay.Card) || Amount <= 0)
        {
            return Task.CompletedTask;
        }

        var hyakuhanhei = Owner.FindPower<FeiyapHyakuhanheiPower>();
        var reduce = hyakuhanhei != null ? FeiyapHyakuhanheiPower.GetAttackIaidoConsume() : 2;
        reduce = Math.Min(Amount, reduce);
        return PowerCmd.Apply(choiceContext, this, Owner, -reduce, Owner, null);
    }

    public override Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player.Creature != Owner)
        {
            return Task.CompletedTask;
        }

        FeiyapCombatTracker.Get(player).OnTurnStart(player);

        if (player.Creature != Owner)
        {
            return Task.CompletedTask;
        }

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
        && IsIaidoBlockableDamage(dealer, props);

    /// <summary>
    /// ???????????????????????? powered attack ??????
    /// </summary>
    private bool IsIaidoBlockableDamage(Creature? dealer, ValueProp props)
    {
        if (props.HasFlag(ValueProp.Unblockable))
        {
            return false;
        }

        if (props.IsPoweredAttack())
        {
            return dealer != null && dealer.Side != Owner.Side;
        }

        return true;
    }

    private bool ShouldIaidoCounter(Creature? dealer, ValueProp props) =>
        dealer != null
        && dealer.Side != Owner.Side
        && props.IsPoweredAttack();

    private void ClearPendingConsume()
    {
        _pendingConsume = 0;
        _pendingIncomingDamage = 0m;
        _pendingCardPlay = null;
    }

    /// <summary>
    /// ??????????????????????????????????????
    /// </summary>
    private decimal ComputeAttackDamageWithoutIaido(
        int consume,
        decimal finalAmountWithIaido,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource,
        CardPlay? cardPlay)
    {
        if (consume <= 0)
        {
            return 0m;
        }

        var multFactor = ComputeDamageMultiplicativeFactor(props, dealer, cardSource, cardPlay);
        if (multFactor <= 0m)
        {
            return 0m;
        }

        // ?????????????????????????????
        var additiveWithIaido = finalAmountWithIaido / multFactor;
        // ????????????????????
        var additiveWithoutIaido = additiveWithIaido + consume;
        return additiveWithoutIaido * multFactor;
    }

    private decimal ComputeDamageMultiplicativeFactor(
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource,
        CardPlay? cardPlay)
    {
        var factor = 1m;
        foreach (var power in Owner.Powers)
        {
            factor *= power.ModifyDamageMultiplicative(Owner, 1m, props, dealer, cardSource, cardPlay);
        }

        return factor;
    }
}
