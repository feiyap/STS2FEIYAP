using Feiyap.Mechanics;
using Feiyap.Powers;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Patching.Models;
using STS2RitsuLib.Scaffolding.Characters;

namespace Feiyap.Patches;

/// <summary>
/// 银碗盛雪生效时，卡牌上的格挡预览改为显示等量的居合获得值。
/// </summary>
public sealed class FeiyapSilverBowlBlockPreviewPatch : IPatchMethod
{
    public static string PatchId => "feiyap_silver_bowl_block_preview";

    public static string Description => "银碗盛雪下格挡预览显示居合值";

    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets() =>
    [
        new(typeof(BlockVar), nameof(BlockVar.UpdateCardPreview), [
            typeof(CardModel),
            typeof(CardPreviewMode),
            typeof(Creature),
            typeof(bool)
        ]),
        new(typeof(CalculatedBlockVar), nameof(CalculatedBlockVar.UpdateCardPreview), [
            typeof(CardModel),
            typeof(CardPreviewMode),
            typeof(Creature),
            typeof(bool)
        ])
    ];

    public static void Postfix(
        DynamicVar __instance,
        CardModel card,
        Creature? target,
        bool runGlobalHooks)
    {
        if (!runGlobalHooks || card.Owner?.Creature is not { } creature)
        {
            return;
        }

        if (creature.FindPower<FeiyapSilverBowlSnowPower>() == null)
        {
            return;
        }

        var baseAmount = __instance switch
        {
            BlockVar blockVar => blockVar.BaseValue,
            CalculatedBlockVar calculatedBlockVar => calculatedBlockVar.Calculate(target),
            _ => (decimal?)null
        };

        if (baseAmount == null)
        {
            return;
        }

        var props = __instance switch
        {
            BlockVar blockVar => blockVar.Props,
            CalculatedBlockVar calculatedBlockVar => calculatedBlockVar.Props,
            _ => default
        };

        __instance.PreviewValue = FeiyapIaidoCmd.PreviewGain(
            creature,
            baseAmount.Value,
            props,
            card);
    }
}
