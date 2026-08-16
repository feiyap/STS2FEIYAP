using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Feiyap.Powers;

/// <summary>
/// 绯神乐：下一张攻击或技能不消耗居合，且攻击额外造成当前居合数值的伤害。
/// </summary>
[RegisterPower]
public sealed class FeiyapScarletKaguraPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override PowerAssetProfile AssetProfile =>
        FeiyapPowerAssets.ForSharedIcon(nameof(FeiyapScarletKaguraPower), "FeiyapSwordSaintHeartPower");

    public override decimal ModifyDamageAdditive(
        Creature? target,
        decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource,
        CardPlay? cardPlay)
    {
        if (dealer != Owner
            || cardSource?.Owner?.Creature != Owner
            || cardSource.Type != CardType.Attack
            || !props.IsPoweredAttack())
        {
            return 0m;
        }

        return Owner.GetPowerAmount<FeiyapIaidoPower>();
    }
}
