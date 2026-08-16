using Feiyap.Cards.Tarot;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Nodes.Cards;
using STS2RitsuLib.Patching.Models;

namespace Feiyap.Patches;

/// <summary>
/// 手牌视觉刷新时同步塔罗正/逆位卡图，使其与当前可触发朝向一致。
/// </summary>
public sealed class FeiyapTarotPortraitRefreshPatch : IPatchMethod
{
    public static string PatchId => "feiyap_tarot_portrait_refresh";

    public static string Description => "塔罗正逆位卡图实时同步";

    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets() =>
    [
        new(typeof(NCard), nameof(NCard.UpdateVisuals), [typeof(PileType), typeof(CardPreviewMode)])
    ];

    public static void Postfix(NCard __instance)
    {
        if (__instance.Model is not FeiyapTarotCardBase tarot)
        {
            return;
        }

        tarot.ApplyPortraitToNode(__instance);
    }
}
