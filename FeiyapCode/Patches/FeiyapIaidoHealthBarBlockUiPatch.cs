using Feiyap.Powers;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Nodes.Combat;
using STS2RitsuLib.Patching.Models;
using STS2RitsuLib.Scaffolding.Characters;

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
        var iaido = creature.FindPower<FeiyapIaidoPower>()?.Amount ?? 0;
        return block > 0 || iaido <= 0;
    }
}
