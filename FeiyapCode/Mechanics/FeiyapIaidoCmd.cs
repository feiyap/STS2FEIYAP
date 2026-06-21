using Feiyap.Powers;
using Feiyap.Relics;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Scaffolding.Characters;

namespace Feiyap.Mechanics;

/// <summary>
/// 居合数值的获得与修正。
/// </summary>
public static class FeiyapIaidoCmd
{
    public static async Task<decimal> Gain(
        PlayerChoiceContext choiceContext,
        Creature creature,
        decimal amount,
        ValueProp props,
        CardModel? cardSource,
        CardPlay? cardPlay)
    {
        if (creature == null || amount <= 0m || CombatManager.Instance.IsOverOrEnding)
        {
            return 0m;
        }

        var modified = amount;

        if (IsDexterityAmplified(props, cardSource, creature))
        {
            modified += creature.GetPowerAmount<DexterityPower>();
        }

        var player = creature.Player;
        if (player != null)
        {
            modified *= GetAlternateIaidoMultiplier(player);
        }

        modified = Math.Max(0m, modified);
        if (modified <= 0m)
        {
            return 0m;
        }

        var zanxin = creature.FindPower<FeiyapZanxinPower>();
        if (zanxin is { Amount: > 0 })
        {
            modified += zanxin.Amount;
        }

        var quickAbsorption = creature.FindPower<FeiyapQuickAbsorptionPower>();
        if (quickAbsorption is { Amount: > 0 })
        {
            modified += quickAbsorption.Amount;
        }

        await PowerCmd.Apply(
            choiceContext,
            ModelDb.Power<FeiyapIaidoPower>().ToMutable(),
            creature,
            modified,
            creature,
            cardSource);

        if (zanxin is { Amount: > 0 })
        {
            await PowerCmd.Apply(choiceContext, zanxin, creature, -zanxin.Amount, creature, cardSource);
        }

        if (player != null)
        {
            FeiyapPerfectIaidoCmd.OnIaidoGained(player);
        }

        return modified;
    }

    public static async Task ClearAll(PlayerChoiceContext choiceContext, Creature creature)
    {
        var power = creature.FindPower<FeiyapIaidoPower>();
        if (power == null || power.Amount <= 0)
        {
            return;
        }

        await PowerCmd.Apply(choiceContext, power, creature, -power.Amount, creature, null);
    }

    private static bool IsDexterityAmplified(ValueProp props, CardModel? cardSource, Creature creature)
    {
        if (!props.HasFlag(ValueProp.Move) || props.HasFlag(ValueProp.Unpowered))
        {
            return false;
        }

        if (cardSource != null)
        {
            return cardSource.Owner.Creature == creature;
        }

        return true;
    }

    private static decimal GetAlternateIaidoMultiplier(Player player)
    {
        if (!FeiyapCombatTracker.Get(player).AlternateBonusActive)
        {
            return 1m;
        }

        foreach (var relic in player.Relics)
        {
            if (relic is KuangXiaoMoNv)
            {
                return 1.5m;
            }

            if (relic is MerryWitch)
            {
                return 1.25m;
            }
        }

        return 1m;
    }
}
