using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Feiyap.Powers;

/// <summary>
/// 血溅五步：活力对该攻击牌发挥多倍效果。
/// </summary>
[RegisterPower]
public sealed class FeiyapBloodSplashVigorPower : ModPowerTemplate
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
        if (dealer != Owner || Amount <= 1m || cardSource is not Cards.Uncommon.FeiyapUncommon5)
        {
            return 0m;
        }

        if (!props.IsPoweredAttack())
        {
            return 0m;
        }

        var vigor = Owner.GetPowerAmount<VigorPower>();
        if (vigor <= 0)
        {
            return 0m;
        }

        return vigor * (Amount - 1m);
    }
}
