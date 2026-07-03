using System.Collections.Generic;
using Godot;

namespace Danjin.Audio;

public sealed class ModSound
{
	private static readonly Dictionary<string, AudioStream> _streamCache = new Dictionary<string, AudioStream>();

	public string File { get; }

	public ModSound(string file)
	{
		File = file;
	}

	public static implicit operator ModSound(string path)
	{
		return new ModSound(path);
	}

	internal AudioStream? GetOrLoadStream()
	{
		if (_streamCache.TryGetValue(File, out var value))
		{
			if (GodotObject.IsInstanceValid((GodotObject)(object)value))
			{
				return value;
			}
			_streamCache.Remove(File);
		}
		AudioStream val = GD.Load<AudioStream>(File);
		if (val != null && val.GetLength() < 15.0)
		{
			_streamCache[File] = val;
		}
		return val;
	}

	public AudioStreamPlayer? Play(float volumeAdd = 0f)
	{
		return DanjinAudio.PlaySoundGlobal(this, volumeAdd);
	}
}
