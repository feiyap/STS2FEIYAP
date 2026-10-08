using System.Reflection;
using System.Threading.Tasks;
using Feiyap.Powers;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Patching.Models;
using STS2RitsuLib.Scaffolding.Characters;

namespace Feiyap.Patches;

/// <summary>
/// 雨曾为紫效果：拥有保留活力能力时，攻击不再消耗活力。
/// </summary>
public sealed class FeiyapPreserveVigorPatch : IPatchMethod
{
    private static readonly FieldInfo? InternalDataField =
        AccessTools.Field(typeof(PowerModel), "_internalData");

    private static readonly FieldInfo? CommandToModifyField =
        AccessTools.Field(
            typeof(VigorPower).GetNestedType("Data", BindingFlags.NonPublic),
            "commandToModify");

    public static string PatchId => "feiyap_preserve_vigor";

    public static string Description => "保留活力时跳过活力消耗";

    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets() =>
    [
        new(typeof(VigorPower), nameof(VigorPower.AfterAttack), [
            typeof(PlayerChoiceContext),
            typeof(AttackCommand)
        ])
    ];

    public static bool Prefix(VigorPower __instance, ref Task __result)
    {
        if (__instance.Owner?.FindPower<FeiyapPreserveVigorPower>() == null)
        {
            return true;
        }

        // AfterAttack 是 async Task。Prefix 返回 false 时必须给出 CompletedTask，
        // 否则 Hook.AfterAttack 会 await 到 null，敌人攻击也会卡死战斗。
        var data = InternalDataField?.GetValue(__instance);
        if (data != null)
        {
            CommandToModifyField?.SetValue(data, null);
        }

        __result = Task.CompletedTask;
        return false;
    }
}
