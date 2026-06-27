using MegaCrit.Sts2.Core.Localization;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Patching.Models;

namespace Feiyap.Patches;

/// <summary>
/// 为 mod 关键词描述注入储君星辉图标变量，使 {singleStarIcon} 能在悬停提示中正确渲染。
/// </summary>
public sealed class FeiyapKeywordDescriptionPatch : IPatchMethod
{
    private const string StarIconMarkup =
        "[img]res://images/packed/sprite_fonts/star_icon.png[/img]";

    public static string PatchId => "feiyap_keyword_description_star_icon";

    public static string Description => "为 mod 关键词描述注入 singleStarIcon";

    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets() =>
    [
        new(typeof(ModKeywordRegistry), nameof(ModKeywordRegistry.GetDescription), [typeof(string)])
    ];

    public static void Postfix(LocString __result)
    {
        __result.Add("singleStarIcon", StarIconMarkup);
    }
}
