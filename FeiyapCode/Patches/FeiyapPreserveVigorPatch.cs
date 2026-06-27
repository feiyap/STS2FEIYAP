using Feiyap.Powers;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Patching.Models;
using STS2RitsuLib.Scaffolding.Characters;

namespace Feiyap.Patches;

/// <summary>
/// 雨曾为紫效果：拥有保留活力能力时，攻击不再消耗活力。
/// </summary>
public sealed class FeiyapPreserveVigorPatch : IPatchMethod
{
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

    public static bool Prefix(VigorPower __instance)
    {
        return __instance.Owner.FindPower<FeiyapPreserveVigorPower>() == null;
    }
}
