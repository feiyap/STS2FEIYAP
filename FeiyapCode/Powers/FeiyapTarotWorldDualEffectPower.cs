using Feiyap.Mechanics;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Feiyap.Powers;

/// <summary>
/// XXI-世界（逆位）：所有塔罗牌耗能增加 1，同时触发正位和逆位效果。
/// </summary>
[RegisterPower]
public sealed class FeiyapTarotWorldDualEffectPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override PowerAssetProfile AssetProfile => FeiyapPowerAssets.For(nameof(FeiyapTarotWorldDualEffectPower));

    public override bool TryModifyEnergyCostInCombatLate(
        CardModel card,
        decimal originalCost,
        out decimal modifiedCost)
    {
        modifiedCost = originalCost;
        if (card.Owner?.Creature != Owner || !FeiyapCardTags.HasTarot(card))
        {
            return false;
        }

        if (card.Pile?.Type is not (PileType.Hand or PileType.Play))
        {
            return false;
        }

        modifiedCost = originalCost + 1m;
        return true;
    }
}
