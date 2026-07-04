using STS2RitsuLib.Patching.Core;
using STS2RitsuLib.Patching.Models;

namespace Feiyap.Patches;

/// <summary>
/// 绯夜氏玩法相关 Harmony 补丁注册入口。
/// </summary>
public sealed class FeiyapGameplayPatches : IModPatches
{
    public static void AddTo(ModPatcher patcher)
    {
        patcher.RegisterPatch<FeiyapDustyTomeSetupPatch>();
        patcher.RegisterPatch<FeiyapPreserveVigorPatch>();
        patcher.RegisterPatch<FeiyapHermitVigorPatch>();
        patcher.RegisterPatch<FeiyapKeywordDescriptionPatch>();
        patcher.RegisterPatch<FeiyapCombatTrackingAfterCardPlayedPatch>();
        patcher.RegisterPatch<FeiyapCombatTrackingAfterDamageGivenPatch>();
        patcher.RegisterPatch<FeiyapCombatTrackingBeforeSideTurnStartPatch>();
        patcher.RegisterPatch<FeiyapCombatTrackingAfterCombatEndPatch>();
    }
}
