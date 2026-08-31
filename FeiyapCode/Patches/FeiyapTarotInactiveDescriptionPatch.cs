using System.Reflection;
using Feiyap.Cards.Tarot;
using Feiyap.Mechanics;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Patching.Models;

namespace Feiyap.Patches;

/// <summary>
/// 战斗中把塔罗牌描述里当前不会生效的正/逆位行置灰。
/// </summary>
public sealed class FeiyapTarotInactiveDescriptionPatch : IPatchMethod
{
    private const string DescriptionPreviewTypeName = "DescriptionPreviewType";

    public static string PatchId => "feiyap_tarot_inactive_description";

    public static string Description => "塔罗未生效正逆位描述置灰";

    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets()
    {
        var previewType = typeof(CardModel).GetNestedType(
                             DescriptionPreviewTypeName,
                             BindingFlags.Public | BindingFlags.NonPublic)
                         ?? throw new MissingMemberException(
                             typeof(CardModel).FullName,
                             DescriptionPreviewTypeName);

        return
        [
            new(typeof(CardModel), nameof(CardModel.GetDescriptionForPile),
                [typeof(PileType), previewType, typeof(Creature)])
        ];
    }

    /// <summary>
    /// 晚于关键词注入，确保置灰作用在最终描述文本上。
    /// </summary>
    [HarmonyPriority(Priority.Last - 1)]
    public static void Postfix(CardModel __instance, ref string __result)
    {
        if (__instance is not FeiyapTarotCardBase tarot || !tarot.IsMutable)
        {
            return;
        }

        // 「世界」正位自选：两张预览牌按各自朝向置灰，不走双效/自选都生效的逻辑。
        if (tarot.IsFreeChoiceOrientationPreview)
        {
            __result = FeiyapTarotInactiveDescription.Apply(
                __result,
                uprightActive: !tarot.IsReversed,
                reversedActive: tarot.IsReversed);
            return;
        }

        if (CombatManager.Instance is not { IsInProgress: true })
        {
            return;
        }

        var player = tarot.Owner;
        if (player == null)
        {
            return;
        }

        __result = FeiyapTarotInactiveDescription.Apply(
            __result,
            tarot.WillUprightEffectApply(player),
            tarot.WillReversedEffectApply(player));
    }
}
