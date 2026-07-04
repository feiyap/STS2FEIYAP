using System.Reflection;
using Feiyap.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Patching.Models;
using STS2RitsuLib.Scaffolding.Characters;

namespace Feiyap.Patches;

/// <summary>
/// 隐者正位：下一次消耗活力时不扣除（等效于返还等量活力）。
/// </summary>
public sealed class FeiyapHermitVigorPatch : IPatchMethod
{
    private static readonly MethodInfo? GetInternalDataMethod =
        typeof(VigorPower).BaseType?.GetMethod("GetInternalData", BindingFlags.Instance | BindingFlags.NonPublic);

    public static string PatchId => "feiyap_hermit_vigor";

    public static string Description => "隐者返还活力";

    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets() =>
    [
        new(typeof(VigorPower), nameof(VigorPower.AfterAttack), [
            typeof(PlayerChoiceContext),
            typeof(AttackCommand)
        ])
    ];

    public static bool Prefix(VigorPower __instance, PlayerChoiceContext choiceContext, AttackCommand command)
    {
        var hermit = __instance.Owner.FindPower<FeiyapHermitPower>();
        if (hermit == null
            || hermit.IsUprightConsumed
            || command.Attacker != __instance.Owner
            || (!hermit.IsDualEffect && hermit.IsReversed))
        {
            return true;
        }

        if (GetInternalDataMethod?.Invoke(__instance, [typeof(object)]) is not { } data)
        {
            return true;
        }

        var commandField = data.GetType().GetField("commandToModify");
        if (commandField?.GetValue(data) as AttackCommand != command)
        {
            return true;
        }

        hermit.MarkUprightConsumed();
        hermit.Flash();
        if (hermit.IsFullyConsumed())
        {
            _ = PowerCmd.Remove(hermit);
        }
        return false;
    }
}
