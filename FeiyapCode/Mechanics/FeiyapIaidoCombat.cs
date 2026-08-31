using System.Linq;
using System.Threading.Tasks;
using Feiyap.Patches;
using Feiyap.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Scaffolding.Characters;

namespace Feiyap.Mechanics;

/// <summary>
/// 居合格挡与反击的共享结算。
/// </summary>
internal static class FeiyapIaidoCombat
{
    public static bool IsBlockableDamage(Creature owner, Creature? dealer, ValueProp props)
    {
        if (props.HasFlag(ValueProp.Unblockable))
        {
            return false;
        }

        if (props.IsPoweredAttack())
        {
            return dealer != null && dealer.Side != owner.Side;
        }

        return true;
    }

    public static bool ShouldCounter(Creature owner, Creature? dealer, ValueProp props) =>
        dealer != null
        && dealer.Side != owner.Side
        && props.IsPoweredAttack();

    /// <summary>模拟指定能力未参与减伤时的最终伤害。</summary>
    public static decimal ComputeDamageWithoutPower(
        AbstractModel self,
        Creature owner,
        decimal preRunningTotal,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource,
        CardPlay? cardPlay)
    {
        var combatState = owner.CombatState;
        var runState = combatState?.RunState;
        if (runState == null)
        {
            return Math.Max(0m, preRunningTotal);
        }

        var num = preRunningTotal;
        var passedSelf = false;

        foreach (var item in runState.IterateHookListeners(combatState))
        {
            if (ReferenceEquals(item, self))
            {
                passedSelf = true;
                continue;
            }

            if (!passedSelf)
            {
                continue;
            }

            num += item.ModifyDamageAdditive(owner, num, props, dealer, cardSource, cardPlay);
        }

        foreach (var item in runState.IterateHookListeners(combatState))
        {
            if (ReferenceEquals(item, self))
            {
                continue;
            }

            num *= item.ModifyDamageMultiplicative(owner, num, props, dealer, cardSource, cardPlay);
        }

        var cap = decimal.MaxValue;
        foreach (var item in runState.IterateHookListeners(combatState))
        {
            if (ReferenceEquals(item, self))
            {
                continue;
            }

            var damageCap = item.ModifyDamageCap(owner, props, dealer, cardSource, cardPlay);
            if (damageCap < cap)
            {
                cap = damageCap;
                if (num > cap)
                {
                    num = cap;
                }
            }
        }

        return Math.Max(0m, num);
    }

    public static async Task ResolveBlocked(
        PlayerChoiceContext choiceContext,
        Creature owner,
        PowerModel flashSource,
        decimal blockedDamage,
        ValueProp props,
        Creature? dealer,
        bool isPerfect)
    {
        if (blockedDamage <= 0m)
        {
            return;
        }

        var shouldCounter = ShouldCounter(owner, dealer, props);
        if (!shouldCounter)
        {
            isPerfect = false;
        }

        var counterBase = blockedDamage;
        if (shouldCounter
            && owner.FindPower<FeiyapScarletKaguraPower>() != null
            && !FeiyapIaidoCmd.IsInfinite(owner))
        {
            // 消耗居合后 Amount 已减少，补回本次消耗以使用触发前的居合值。
            counterBase = FeiyapIaidoCmd.GetNumericAmount(owner) + blockedDamage;
        }

        var counterDamage = shouldCounter
            ? FeiyapIaidoCmd.ApplyCounterDamageMultiplier(owner, counterBase, isPerfect)
            : 0m;

        flashSource.Flash();
        IaidoHealthBarOverlay.RefreshForCreature(owner);
        owner.FindPower<FeiyapGuilianMoonPower>()?.RecordTrigger();

        if (!shouldCounter)
        {
            return;
        }

        if (isPerfect)
        {
            await FeiyapPerfectIaidoCmd.Trigger(choiceContext, owner, blockedDamage);
        }

        var hitAllEnemies = owner.FindPower<FeiyapHeavenMayPower>() != null
            || owner.FindPower<FeiyapSwordSaintHeartPower>() != null;
        if (hitAllEnemies)
        {
            var enemies = owner.CombatState?.GetOpponentsOf(owner).Where(e => e.IsAlive).ToList() ?? [];
            await FeiyapSlashCmd.PlayIaidoCounterSlashAll(enemies, async () =>
            {
                foreach (var enemy in enemies)
                {
                    await CreatureCmd.Damage(
                        choiceContext,
                        enemy,
                        counterDamage,
                        ValueProp.Unpowered | ValueProp.SkipHurtAnim,
                        owner,
                        null,
                        null);
                }

                if (owner.Player != null && enemies.Count > 0)
                {
                    FeiyapQuestProgress.RecordIaidoDamage(
                        owner.Player,
                        (int)Math.Round(counterDamage * enemies.Count));
                }
            }, isPerfect);
        }
        else if (dealer != null && dealer.Side != owner.Side && dealer.IsAlive)
        {
            await FeiyapSlashCmd.PlayIaidoCounterSlash(dealer, async () =>
            {
                await CreatureCmd.Damage(
                    choiceContext,
                    dealer,
                    counterDamage,
                    ValueProp.Unpowered | ValueProp.SkipHurtAnim,
                    owner,
                    null,
                    null);

                if (owner.Player != null)
                {
                    FeiyapQuestProgress.RecordIaidoDamage(owner.Player, (int)Math.Round(counterDamage));
                }
            }, isPerfect);
        }
    }
}
