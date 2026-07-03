using System.Reflection;
using Danjin.Cards;
using Danjin.Config;
using Danjin.Vfx;
using Godot.Bridge;
using HarmonyLib;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Modding;
using STS2RitsuLib.Interop;

namespace Danjin;

[ModInitializer("Initialize")]
public class MainFile
{
	public const string ModId = "Danjin";

	public static Logger Logger { get; } = new Logger("Danjin", (LogType)0);

	public static void Initialize()
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		DanjinCardKeywords.RegisterAll();
		ModTypeDiscoveryHub.RegisterModAssembly("Danjin", Assembly.GetExecutingAssembly());
		new Harmony("Danjin").PatchAll();
		DanjinConfig.Register();
		ScriptManagerBridge.LookupScriptsInAssembly(typeof(MainFile).Assembly);
		NDimensionSlashVfx.Initialize("res://Danjin/Scenes/danjin_dimension_slash.tscn");
		Log.Debug("Mod initialized!", 2);
	}
}
