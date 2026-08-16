using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Feiyap.Powers;

/// <summary>
/// 担刀势：攻击牌造成的伤害提升 25%。
/// </summary>
[RegisterPower]
public sealed class FeiyapTandaoStancePower : ModPowerTemplate
{
    private const decimal AttackDamageMultiplier = 1.25m;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override PowerAssetProfile AssetProfile =>
        FeiyapPowerAssets.ForSharedIcon(nameof(FeiyapTandaoStancePower), "FeiyapSwordSaintHeartPower");

    public override decimal ModifyDamageMultiplicative(
        Creature? target,
        decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource,
        CardPlay? cardPlay)
    {
        if (dealer != Owner || cardSource?.Type != CardType.Attack || cardSource.Owner?.Creature != Owner)
        {
            return 1m;
        }

        return AttackDamageMultiplier;
    }
}
