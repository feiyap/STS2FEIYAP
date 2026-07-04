using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Feiyap.Powers;

/// <summary>
/// 天下五剑：升级后力量与活力在该牌上发挥 2 倍效果。
/// </summary>
[RegisterPower]
public sealed class FeiyapTenkaFiveSwordsBoostPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override decimal ModifyDamageAdditive(
        Creature? target,
        decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        if (dealer != Owner || Amount <= 1m || cardSource is not Cards.Rare.FeiyapRare1)
        {
            return 0m;
        }

        if (!props.IsPoweredAttack())
        {
            return 0m;
        }

        var bonus = 0m;
        var strength = Owner.GetPowerAmount<StrengthPower>();
        if (strength > 0m)
        {
            bonus += strength * (Amount - 1m);
        }

        var vigor = Owner.GetPowerAmount<VigorPower>();
        if (vigor > 0m)
        {
            bonus += vigor * (Amount - 1m);
        }

        return bonus;
    }
}
