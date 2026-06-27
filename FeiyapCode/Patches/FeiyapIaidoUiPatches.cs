using STS2RitsuLib.Patching.Core;
using STS2RitsuLib.Patching.Models;

namespace Feiyap.Patches;

/// <summary>
/// 居合 UI 相关 Harmony 补丁注册入口。
/// </summary>
public sealed class FeiyapIaidoUiPatches : IModPatches
{
    public static void AddTo(ModPatcher patcher)
    {
        patcher.RegisterPatch<FeiyapIaidoHealthBarRefreshPatch>();
        patcher.RegisterPatch<FeiyapIaidoHealthBarBlockUiPatch>();
        patcher.RegisterPatch<FeiyapIaidoIntentPreviewPatch>();
    }
}
