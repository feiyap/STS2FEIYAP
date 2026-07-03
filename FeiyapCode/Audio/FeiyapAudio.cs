using Godot;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Saves;

namespace Feiyap.Audio;

/// <summary>
/// 在全局场景树播放 Mod 音效，并尊重游戏主音量与 SFX 音量设置。
/// </summary>
public static class FeiyapAudio
{
    public static AudioStreamPlayer? PlaySoundGlobal(FeiyapModSound sound, float volumeAddDb = 0f)
    {
        var stream = sound.GetOrLoadStream();
        if (stream == null)
        {
            Log.Warn($">>>[Feiyap] 加载音频流失败: {sound.File}", 2);
            return null;
        }

        var settings = SaveManager.Instance?.SettingsSave;
        if (settings == null || settings.VolumeMaster <= 0f || settings.VolumeSfx < 0f)
        {
            return null;
        }

        var root = Engine.GetMainLoop() is SceneTree tree ? tree.Root : null;
        if (root == null)
        {
            return null;
        }

        var player = new AudioStreamPlayer
        {
            Name = sound.File,
            Stream = stream,
            VolumeDb = Mathf.LinearToDb(settings.VolumeMaster * settings.VolumeSfx) + volumeAddDb
        };

        root.AddChild(player);
        player.Play();
        player.Finished += () =>
        {
            if (GodotObject.IsInstanceValid(player))
            {
                player.QueueFree();
            }
        };

        return player;
    }
}
