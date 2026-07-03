using System.Collections.Generic;
using Godot;

namespace Feiyap.Audio;

/// <summary>
/// Mod 自定义音效资源引用，通过 Godot 资源路径加载并缓存。
/// </summary>
public sealed class FeiyapModSound
{
    private static readonly Dictionary<string, AudioStream> StreamCache = new();

    public string File { get; }

    public FeiyapModSound(string file) => File = file;

    public static implicit operator FeiyapModSound(string path) => new(path);

    internal AudioStream? GetOrLoadStream()
    {
        if (StreamCache.TryGetValue(File, out var cached) && GodotObject.IsInstanceValid(cached))
        {
            return cached;
        }

        if (StreamCache.ContainsKey(File))
        {
            StreamCache.Remove(File);
        }

        var stream = GD.Load<AudioStream>(File);
        if (stream != null && stream.GetLength() < 15.0)
        {
            StreamCache[File] = stream;
        }

        return stream;
    }

    public AudioStreamPlayer? Play(float volumeAddDb = 0f) =>
        FeiyapAudio.PlaySoundGlobal(this, volumeAddDb);
}
