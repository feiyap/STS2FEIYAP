using MegaCrit.Sts2.Core.Nodes.Combat;
using STS2RitsuLib.Patching.Models;

namespace Feiyap.Patches;

/// <summary>
/// 生命条刷新时套用居合格挡槽 UI。
/// </summary>
public sealed class FeiyapIaidoHealthBarRefreshPatch : IPatchMethod
{
    public static string PatchId => "feiyap_iaido_healthbar_refresh";

    public static string Description => "居合生命条 UI 刷新";

    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets() =>
    [
        new(typeof(NHealthBar), nameof(NHealthBar.RefreshValues))
    ];

    public static void Postfix(NHealthBar __instance) =>
        IaidoHealthBarOverlay.Apply(__instance);
}
