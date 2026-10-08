using System;
using System.Threading.Tasks;
using Feiyap.Audio;
using Godot;
using MegaCrit.Sts2.Core.Audio.Debug;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.TestSupport;

namespace Feiyap.Vfx;

/// <summary>
/// 全屏空间斩裂特效（参考良秀 Boss 斩屏）：枯山水等卡牌使用。
/// </summary>
public static class FeiyapSpaceSeverVfx
{
    public const float Duration = 3.15f;

    private static readonly string ShaderPath = $"{Entry.ResPath}/Shaders/feiyap_space_sever.gdshader";

    private static readonly FeiyapModSound SeverLayerSound =
        new($"{Entry.ResPath}/Sounds/iaido_slash.wav");

    private static CanvasLayer? _layer;

    /// <summary>以目标为中心播放；目标为空时使用默认屏幕中心。</summary>
    public static void PlayCenteredOn(Creature? target)
    {
        if (TestMode.IsOn || NonInteractiveMode.IsActive)
        {
            return;
        }

        var center = new Vector2(0.62f, 0.48f);
        try
        {
            var room = NCombatRoom.Instance;
            var creatureNode = target != null && room != null
                ? room.GetCreatureNode(target)
                : null;
            if (creatureNode != null && room != null)
            {
                var canvasPos = creatureNode.GetViewport().GetCanvasTransform()
                    * creatureNode.VfxSpawnPosition;
                var size = room.GetViewport().GetVisibleRect().Size;
                if (size.X > 0f && size.Y > 0f)
                {
                    center = new Vector2(
                        Mathf.Clamp(canvasPos.X / size.X, 0.35f, 0.75f),
                        Mathf.Clamp(canvasPos.Y / size.Y, 0.35f, 0.6f));
                }
            }
        }
        catch (Exception ex)
        {
            Log.Warn("[Feiyap] 斩屏中心计算失败: " + ex.Message, 2);
        }

        Play(center);
    }

    /// <summary>枯山水：播放斩屏与音效。</summary>
    public static Task PlayForKaresansuiAsync(Creature? preferredTarget)
    {
        if (!TestMode.IsOn && !NonInteractiveMode.IsActive)
        {
            PlaySeverSfx();
        }

        PlayCenteredOn(preferredTarget);
        return Task.CompletedTask;
    }

    private static void PlaySeverSfx()
    {
        SfxCmd.Play(TmpSfx.slashAttack);
        SfxCmd.Play(TmpSfx.heavyAttack);
        SeverLayerSound.Play();
    }

    private static void Play(Vector2 center)
    {
        if (TestMode.IsOn || NonInteractiveMode.IsActive)
        {
            return;
        }

        var room = NCombatRoom.Instance;
        if (room == null || !GodotObject.IsInstanceValid(room))
        {
            return;
        }

        if (_layer != null && GodotObject.IsInstanceValid(_layer))
        {
            _layer.QueueFree();
            _layer = null;
        }

        CanvasLayer? layer = null;
        try
        {
            var shader = ResourceLoader.Load<Shader>(ShaderPath);
            if (shader == null)
            {
                Log.Warn("[Feiyap] 无法加载斩屏 Shader: " + ShaderPath, 2);
                return;
            }

            layer = new CanvasLayer
            {
                Name = "FeiyapSpaceSever",
                Layer = 1
            };
            _layer = layer;
            room.AddChildSafely(layer);

            layer.AddChildSafely(new BackBufferCopy
            {
                CopyMode = BackBufferCopy.CopyModeEnum.Viewport,
                Name = "FeiyapSpaceSeverBackBuffer"
            });

            var material = new ShaderMaterial
            {
                Shader = shader
            };
            material.SetShaderParameter("cut_center", center);
            material.SetShaderParameter("elapsed", 0f);

            var rect = new ColorRect
            {
                Name = "SpaceSeverScreen",
                MouseFilter = Control.MouseFilterEnum.Ignore,
                FocusMode = Control.FocusModeEnum.None,
                Material = material,
                Color = Colors.White
            };
            layer.AddChildSafely(rect);
            rect.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);

            var active = layer;
            active.TreeExiting += () =>
            {
                if (_layer == active)
                {
                    _layer = null;
                }
            };

            var tween = rect.CreateTween();
            tween.TweenMethod(
                Callable.From<float>(time =>
                {
                    if (GodotObject.IsInstanceValid(rect))
                    {
                        material.SetShaderParameter("elapsed", time);
                    }
                }),
                0f,
                Duration,
                Duration);
            tween.TweenCallback(Callable.From(() =>
            {
                if (GodotObject.IsInstanceValid(active))
                {
                    active.QueueFree();
                }
            }));
        }
        catch (Exception ex)
        {
            if (layer != null && GodotObject.IsInstanceValid(layer))
            {
                layer.QueueFree();
            }

            _layer = null;
            Log.Warn("[Feiyap] 斩屏特效跳过: " + ex.Message, 2);
        }
    }

    public static void Reset()
    {
        if (_layer != null && GodotObject.IsInstanceValid(_layer))
        {
            _layer.QueueFree();
        }

        _layer = null;
    }
}
