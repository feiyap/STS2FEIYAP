using System.Linq;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Feiyap.Powers;

/// <summary>
/// 立处皆真：无负面能力时造成的伤害提升（Amount 为百分比加算，50=×1.5，100=×2.0）。
/// </summary>
[RegisterPower]
public sealed class FeiyapStandingTruthPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override PowerAssetProfile AssetProfile =>
        FeiyapPowerAssets.ForSharedIcon(nameof(FeiyapStandingTruthPower), "FeiyapSwordSaintHeartPower");

    public override decimal ModifyDamageMultiplicative(
        Creature? target,
        decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource,
        CardPlay? cardPlay)
    {
        if (dealer != Owner || cardSource?.Owner?.Creature != Owner || Amount <= 0)
        {
            return 1m;
        }

        if (Owner.Powers.Any(p => p.Type == PowerType.Debuff))
        {
            return 1m;
        }

        return 1m + Amount / 100m;
    }
}
