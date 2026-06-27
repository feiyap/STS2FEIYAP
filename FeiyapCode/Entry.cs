using System.Reflection;
using Feiyap.Mechanics;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Modding;
using Feiyap.Patches;
using STS2RitsuLib;
using STS2RitsuLib.CardTags;
using STS2RitsuLib.Interop;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Patching.Core;
using Logger = MegaCrit.Sts2.Core.Logging.Logger;

namespace Feiyap;

[ModInitializer(nameof(Initialize))]
public partial class Entry
{
    // ModId 需要和 Feiyap.json 里的 id 保持一致。
    // res://Feiyap/... 里的 Feiyap 是 PCK 资源目录，不是 C# namespace。
    public const string ModId = "Feiyap";
    public const string ResPath = $"res://{ModId}";

    public static Logger Logger { get; } = new(ModId, LogType.Generic);

    public static void Initialize()
    {
        var assembly = Assembly.GetExecutingAssembly();

        FeiyapCardTags.Register(ModCardTagRegistry.For(ModId));
        FeiyapKeywords.Register(ModKeywordRegistry.For(ModId));

        // 以下示例默认已经在 Entry.Initialize() 中调用了
        // RitsuLibFramework.EnsureGodotScriptsRegistered(...) 和
        // ModTypeDiscoveryHub.RegisterModAssembly(...)，否则自动注册不会生效。
        //
        // Godot C# 脚本注册只负责让 pck 中的脚本类型能被 Godot 找到。
        // 这一步和 RitsuLib 的内容自动注册不是同一件事，两个都需要保留。
        RitsuLibFramework.EnsureGodotScriptsRegistered(assembly, Logger);

        // 自动注册扫描会读取当前程序集里的 RegisterCard/RegisterRelic 等 attribute。
        // 新增内容类后，只要 attribute 写对，通常不需要在入口里手动逐个注册。
        ModTypeDiscoveryHub.RegisterModAssembly(ModId, assembly);

        var iaidoUiPatcher = RitsuLibFramework.CreatePatcher(ModId, "iaido-ui");
        iaidoUiPatcher.RegisterPatches<FeiyapIaidoUiPatches>();
        RitsuLibFramework.ApplyRequiredPatcher(
            iaidoUiPatcher,
            () => Logger.Warn("居合 UI 补丁应用失败，相关显示可能不可用。"));

        var gameplayPatcher = RitsuLibFramework.CreatePatcher(ModId, "gameplay");
        gameplayPatcher.RegisterPatches<FeiyapGameplayPatches>();
        RitsuLibFramework.ApplyRequiredPatcher(
            gameplayPatcher,
            () => Logger.Warn("玩法补丁应用失败，保留活力可能不可用。"));

        Logger.Info("Feiyap initialized.");
    }
}