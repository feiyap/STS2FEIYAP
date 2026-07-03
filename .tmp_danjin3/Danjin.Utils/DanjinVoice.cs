using Danjin.Audio;
using Danjin.Config;
using Godot;

namespace Danjin.Utils;

public static class DanjinVoice
{
	private static AudioStreamPlayer? _current;

	private const float BaseVolumeAdd = 17f;

	private static float GetVolumeAdd()
	{
		int voiceVolume = DanjinConfig.VoiceVolume;
		if (voiceVolume <= 0)
		{
			return float.NegativeInfinity;
		}
		if (voiceVolume >= 100)
		{
			return 17f;
		}
		return 17f + 20f * Mathf.Log((float)voiceVolume / 100f) / Mathf.Log(10f);
	}

	public static void Play(ModSound sound)
	{
		if (_current != null && GodotObject.IsInstanceValid((GodotObject)(object)_current) && _current.Playing)
		{
			_current.Stop();
		}
		float volumeAdd = GetVolumeAdd();
		if (!float.IsNegativeInfinity(volumeAdd))
		{
			_current = DanjinAudio.PlaySoundGlobal(sound, volumeAdd);
		}
	}
}
