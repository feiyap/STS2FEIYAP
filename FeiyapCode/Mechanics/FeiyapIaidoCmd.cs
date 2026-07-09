using Feiyap.Patches;
using Feiyap.Relics;
using Feiyap.Powers;
using System.Linq;
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
    /// <summary>计算居合获得预览值（敏捷、残心、战斗加成等），不实际施加。</summary>
    public static decimal PreviewGain(
        Creature creature,
        decimal amount,
        ValueProp props,
        CardModel? cardSource)
    {
        var (_, _, total) = ComputeGainAmount(creature, amount, props, cardSource);
        return total;
    }

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

        var (scaledAmount, additives, modified) = ComputeGainAmount(creature, amount, props, cardSource);
        if (modified <= 0m)
        {
            return 0m;
        }

        var context = new FeiyapIaidoGainContext
        {
            ChoiceContext = choiceContext,
            Creature = creature,
            BaseAmount = amount,
            ScaledAmount = scaledAmount,
            Props = props,
            CardSource = cardSource,
            CardPlay = cardPlay
        };

        SfxCmd.Play("event:/sfx/block_gain");

        await PowerCmd.Apply(
            choiceContext,
            ModelDb.Power<FeiyapIaidoPower>().ToMutable(),
            creature,
            modified,
            creature,
            cardSource);

        foreach (var (source, bonus) in additives)
        {
            if (bonus > 0m)
            {
                await source.OnIaidoGainApplied(context, bonus);
            }
        }

        if (creature.Player != null)
        {
            await FeiyapKassaiJizaiPower.OnIaidoGained(choiceContext, creature, modified);
        }

        if (creature.Player != null)
        {
            FeiyapPerfectIaidoCmd.OnIaidoGained(creature.Player);
        }

        IaidoHealthBarOverlay.RefreshForCreature(creature);
        return modified;
    }

    /// <summary>居合反击伤害乘算（如斋时雨翻倍、完美居合遗物增幅）。</summary>
    public static decimal ApplyCounterDamageMultiplier(Creature creature, decimal baseDamage, bool isPerfect)
    {
        if (baseDamage <= 0m)
        {
            return 0m;
        }

        var damage = baseDamage;

        if (creature.FindPower<FeiyapIaidoRainPower>() != null)
        {
            damage *= 2m;
        }

        var forcedPerfect = creature.FindPower<FeiyapIaidoSurgePower>() != null;
        if (creature.Player is { } player && (forcedPerfect || isPerfect))
        {
            foreach (var relic in player.Relics.OfType<SwordSaintBase>())
            {
                damage *= 1m + relic.PerfectIaidoDamageBonusPercent / 100m;
            }
        }

        return damage;
    }

    public static async Task ClearAll(PlayerChoiceContext choiceContext, Creature creature)
    {
        var power = creature.FindPower<FeiyapIaidoPower>();
        if (power == null || power.Amount <= 0)
        {
            return;
        }

        await PowerCmd.Apply(choiceContext, power, creature, -power.Amount, creature, null);
        IaidoHealthBarOverlay.RefreshForCreature(creature);
    }

    private static (
        decimal ScaledAmount,
        List<(IFeiyapIaidoGainAdditive Source, decimal Bonus)> Additives,
        decimal TotalAmount) ComputeGainAmount(
        Creature creature,
        decimal amount,
        ValueProp props,
        CardModel? cardSource)
    {
        if (amount <= 0m)
        {
            return (0m, [], 0m);
        }

        var modified = amount;

        if (IsDexterityAmplified(props, cardSource, creature))
        {
            modified += creature.GetPowerAmount<DexterityPower>();
        }

        var context = new FeiyapIaidoGainContext
        {
            ChoiceContext = null!,
            Creature = creature,
            BaseAmount = amount,
            ScaledAmount = modified,
            Props = props,
            CardSource = cardSource,
            CardPlay = null
        };

        modified = ApplyMultiplicativeModifiers(creature.Player, context, modified);
        modified = Math.Max(0m, modified);

        if (creature.Player != null)
        {
            modified += FeiyapCombatTracker.Get(creature.Player).IaidoGainCombatBonus;
        }

        var scaledAmount = modified;
        context = context with { ScaledAmount = scaledAmount };

        var additives = CollectAdditiveModifiers(creature, creature.Player, context);
        foreach (var (_, bonus) in additives)
        {
            modified += bonus;
        }

        return (scaledAmount, additives, Math.Max(0m, modified));
    }

    private static decimal ApplyMultiplicativeModifiers(
        Player? player,
        in FeiyapIaidoGainContext context,
        decimal amount)
    {
        if (player == null)
        {
            return amount;
        }

        foreach (var relic in player.Relics)
        {
            if (relic is IFeiyapIaidoGainMultiplier multiplier)
            {
                amount = multiplier.ModifyIaidoGainMultiplicative(context, amount);
            }
        }

        foreach (var power in context.Creature.Powers)
        {
            if (power is IFeiyapIaidoGainMultiplier multiplier)
            {
                amount = multiplier.ModifyIaidoGainMultiplicative(context, amount);
            }
        }

        return amount;
    }

    private static List<(IFeiyapIaidoGainAdditive Source, decimal Bonus)> CollectAdditiveModifiers(
        Creature creature,
        Player? player,
        in FeiyapIaidoGainContext context)
    {
        var additives = new List<(IFeiyapIaidoGainAdditive Source, decimal Bonus)>();

        foreach (var power in creature.Powers)
        {
            if (power is not IFeiyapIaidoGainAdditive additive)
            {
                continue;
            }

            var bonus = additive.GetIaidoGainAdditiveBonus(context);
            if (bonus > 0m)
            {
                additives.Add((additive, bonus));
            }
        }

        if (player == null)
        {
            return additives;
        }

        foreach (var relic in player.Relics)
        {
            if (relic is not IFeiyapIaidoGainAdditive additive)
            {
                continue;
            }

            var bonus = additive.GetIaidoGainAdditiveBonus(context);
            if (bonus > 0m)
            {
                additives.Add((additive, bonus));
            }
        }

        return additives;
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
}
