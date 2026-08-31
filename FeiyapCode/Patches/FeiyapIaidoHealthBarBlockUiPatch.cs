using Feiyap.Mechanics;
using MegaCrit.Sts2.Core.Nodes.Combat;
using STS2RitsuLib.Patching.Models;

namespace Feiyap.Patches;

/// <summary>
/// 仅有居合、无格挡时阻止原版隐藏格挡 UI 容器。
/// </summary>
public sealed class FeiyapIaidoHealthBarBlockUiPatch : IPatchMethod
{
    public static string PatchId => "feiyap_iaido_healthbar_block_ui";

    public static string Description => "居合存在时保持格挡槽可见";

    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets() =>
    [
        new(typeof(NHealthBar), "RefreshBlockUi")
    ];

    public static bool Prefix(NHealthBar __instance)
    {
        var creature = IaidoHealthBarOverlay.GetCreature(__instance);
        if (creature == null || !IaidoHealthBarOverlay.IsFeiyapPlayer(creature))
        {
            return true;
        }

        var block = Math.Max(0, creature.Block);
        // 无限居合同样占用格挡槽；若放行原版 RefreshBlockUi，会因 Block==0 反复播放碎裂 VFX。
        return block > 0 || !FeiyapIaidoCmd.HasIaido(creature);
    }
}
