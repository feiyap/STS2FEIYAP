using Godot;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Saves;

namespace Danjin.Audio;

public static class DanjinAudio
{
	public static AudioStreamPlayer? PlaySoundGlobal(ModSound sound, float volumeAdd = 0f)
	{
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Expected O, but got Unknown
		AudioStream orLoadStream = sound.GetOrLoadStream();
		if (orLoadStream == null)
		{
			Log.Warn(">>>[DanjinMod] DanjinAudio: 加载音频流失败 " + sound.File, 2);
			return null;
		}
		SaveManager instance = SaveManager.Instance;
		SettingsSave val = ((instance != null) ? instance.SettingsSave : null);
		if (val == null)
		{
			return null;
		}
		if (val.VolumeMaster <= 0f)
		{
			return null;
		}
		if (val.VolumeSfx < 0f)
		{
			return null;
		}
		MainLoop mainLoop = Engine.GetMainLoop();
		MainLoop obj = ((mainLoop is SceneTree) ? mainLoop : null);
		Window val2 = ((obj != null) ? ((SceneTree)obj).Root : null);
		if (val2 == null)
		{
			return null;
		}
		AudioStreamPlayer player = new AudioStreamPlayer
		{
			Name = StringName.op_Implicit(sound.File),
			Stream = orLoadStream,
			VolumeDb = Mathf.LinearToDb(val.VolumeMaster * val.VolumeSfx) + volumeAdd
		};
		((Node)val2).AddChild((Node)(object)player, false, (InternalMode)0);
		player.Play(0f);
		player.Finished += delegate
		{
			if (GodotObject.IsInstanceValid((GodotObject)(object)player))
			{
				((Node)player).QueueFree();
			}
		};
		return player;
	}
}
