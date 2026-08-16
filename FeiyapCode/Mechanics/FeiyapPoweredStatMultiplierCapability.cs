using Feiyap.Cards.Rare;
using Feiyap.Cards.Uncommon;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Models.Capabilities;

namespace Feiyap.Mechanics;

/// <summary>
/// 卡牌能力基类：放大该卡 powered 攻击从力量/活力获得的额外伤害，而非通过角色身上的临时 Power 实现。
/// </summary>
public abstract class FeiyapPoweredStatMultiplierCapabilityBase : CardCapability
{
    protected abstract string MultiplierVarName { get; }

    protected abstract bool AffectStrength { get; }

    protected abstract bool AffectVigor { get; }

    protected virtual bool RequiresUpgrade => false;

    public override decimal ModifyDamageAdditive(
        Creature? target,
        decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource,
        CardPlay? cardPlay)
    {
        if (Owner is not CardModel card
            || cardSource != card
            || dealer != card.Owner.Creature
            || !props.IsPoweredAttack())
        {
            return 0m;
        }

        if (RequiresUpgrade && !card.IsUpgraded)
        {
            return 0m;
        }

        var multiplier = card.DynamicVars[MultiplierVarName].BaseValue;
        if (multiplier <= 1m)
        {
            return 0m;
        }

        var extraMultiplier = multiplier - 1m;
        var bonus = 0m;

        if (AffectStrength)
        {
            var strength = card.Owner.Creature.GetPowerAmount<StrengthPower>();
            if (strength > 0m)
            {
                bonus += strength * extraMultiplier;
            }
        }

        // 原版活力在 AfterAttack 才扣除，多段攻击每一击都应吃到倍率。
        if (AffectVigor)
        {
            var vigor = card.Owner.Creature.GetPowerAmount<VigorPower>();
            if (vigor > 0m)
            {
                bonus += vigor * extraMultiplier;
            }
        }

        return bonus;
    }
}

/// <summary>
/// 血溅五步：活力在该牌上发挥倍率效果。
/// </summary>
[RegisterModelCapability]
[RegisterDefaultModelCapability(typeof(FeiyapUncommon5))]
public sealed class FeiyapUncommon5PoweredStatMultiplierCapability : FeiyapPoweredStatMultiplierCapabilityBase
{
    protected override string MultiplierVarName => "FeiyapBloodSplashVigorPower";

    protected override bool AffectStrength => false;

    protected override bool AffectVigor => true;
}

/// <summary>
/// 天下五剑：升级后力量与活力在该牌上发挥倍率效果。
/// </summary>
[RegisterModelCapability]
[RegisterDefaultModelCapability(typeof(FeiyapRare1))]
public sealed class FeiyapRare1PoweredStatMultiplierCapability : FeiyapPoweredStatMultiplierCapabilityBase
{
    protected override string MultiplierVarName => "FeiyapTenkaFiveSwordsBoostPower";

    protected override bool AffectStrength => true;

    protected override bool AffectVigor => true;

    protected override bool RequiresUpgrade => true;
}
