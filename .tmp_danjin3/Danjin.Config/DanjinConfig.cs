using System;
using System.Collections.Generic;
using MegaCrit.Sts2.Core.Logging;
using STS2RitsuLib;
using STS2RitsuLib.Data;
using STS2RitsuLib.Settings;
using STS2RitsuLib.Utils.Persistence;
using STS2RitsuLib.Utils.Persistence.Migration;

namespace Danjin.Config;

public static class DanjinConfig
{
	private const string DataKey = "settings";

	private const int DefaultVoiceVolume = 100;

	public static int VoiceVolume
	{
		get
		{
			try
			{
				return ModDataStore.For("Danjin").Get<DanjinSettings>("settings").VoiceVolume;
			}
			catch (Exception ex)
			{
				Log.Warn($">>>[DanjinMod] DanjinConfig.VoiceVolume 读取失败: {ex.Message}, 使用默认 {100}", 2);
				return 100;
			}
		}
	}

	internal static void Register()
	{
		ModDataStore.For("Danjin").Register<DanjinSettings>("settings", "settings", (SaveScope)0, (Func<DanjinSettings>)(() => new DanjinSettings()), false, (ModDataMigrationConfig)null, (IEnumerable<IMigration>)null);
		RitsuLibFramework.RegisterModSettings("Danjin", (Action<ModSettingsPageBuilder>)delegate(ModSettingsPageBuilder page)
		{
			page.WithTitle(ModSettingsText.Literal("丹瑾")).WithModDisplayName(ModSettingsText.Literal("丹瑾")).AddSection("voice", (Action<ModSettingsSectionBuilder>)delegate(ModSettingsSectionBuilder section)
			{
				section.WithTitle(ModSettingsText.Literal("语音")).AddIntSlider("voice_volume", ModSettingsText.Literal("人物语音音量"), (IModSettingsValueBinding<int>)(object)ModSettingsBindings.Global<DanjinSettings, int>("Danjin", "settings", (Func<DanjinSettings, int>)((DanjinSettings s) => s.VoiceVolume), (Action<DanjinSettings, int>)delegate(DanjinSettings s, int value)
				{
					s.VoiceVolume = value;
				}), 0, 100, 5, (Func<int, string>)null, (ModSettingsText)null);
			});
		}, (string)null);
	}
}
