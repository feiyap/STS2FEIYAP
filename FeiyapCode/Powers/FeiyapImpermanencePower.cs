using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Feiyap.Powers;

/// <summary>
/// 灼莲：攻击牌减少 1 点耗能。
/// </summary>
[RegisterPower]
public sealed class FeiyapImpermanencePower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override PowerAssetProfile AssetProfile =>
        FeiyapPowerAssets.ForSharedIcon(nameof(FeiyapImpermanencePower), "FeiyapSwordSaintHeartPower");

    public override bool TryModifyEnergyCostInCombatLate(
        CardModel card,
        decimal originalCost,
        out decimal modifiedCost)
    {
        modifiedCost = originalCost;
        if (card.Owner?.Creature != Owner || card.Type != CardType.Attack)
        {
            return false;
        }

        if (card.Pile?.Type is not (PileType.Hand or PileType.Play))
        {
            return false;
        }

        modifiedCost = System.Math.Max(0m, originalCost - 1m);
        return true;
    }
}
