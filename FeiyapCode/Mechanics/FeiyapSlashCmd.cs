using Feiyap.Audio;
using Feiyap.Vfx;
using Godot;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace Feiyap.Mechanics;

/// <summary>
/// 多维斩击特效辅助：居合反击触发时播放剑气斩击 VFX。
/// </summary>
public static class FeiyapSlashCmd
{
    private static readonly FeiyapModSound[] IaidoSlashSounds =
    [
        new($"{Entry.ResPath}/Sounds/iaido_1.mp3"),
        new($"{Entry.ResPath}/Sounds/iaido_2.mp3"),
        new($"{Entry.ResPath}/Sounds/iaido_3.mp3"),
        new($"{Entry.ResPath}/Sounds/iaido_4.mp3")
    ];

    private static readonly FeiyapModSound[] PerfectIaidoSlashSounds =
    [
        new($"{Entry.ResPath}/Sounds/perfect_iaido_1.mp3"),
        new($"{Entry.ResPath}/Sounds/perfect_iaido_2.mp3")
    ];
    private enum SlashAreaMode
    {
        TargetHitbox,
        TargetExpanded,
        FullScreen
    }

    /// <summary>在 mod 启动时预热斩击场景。</summary>
    public static void Initialize() =>
        NFeiyapDimensionSlashVfx.Initialize($"{Entry.ResPath}/scenes/vfx/feiyap_dimension_slash.tscn");

    /// <summary>居合单体反击：先播放斩击，再执行反击伤害。</summary>
    public static async Task PlayIaidoCounterSlash(Creature? target, Func<Task> onCounter, bool isPerfect = false)
    {
        if (target == null)
        {
            await onCounter();
            return;
        }

        var slashVfx = CreateJianQiSlashAt(target);
        PlayIaidoSlashSound(isPerfect);
        slashVfx?.DoSlash();
        await onCounter();
        slashVfx?.ForceComplete();
    }

    /// <summary>居合群体反击：先对每个目标播放斩击，再执行反击伤害。</summary>
    public static async Task PlayIaidoCounterSlashAll(
        IReadOnlyList<Creature> targets,
        Func<Task> onCounter,
        bool isPerfect = false)
    {
        var slashVfxList = new List<NFeiyapDimensionSlashVfx?>();
        var playedSound = false;
        foreach (var target in targets)
        {
            if (!target.IsAlive)
            {
                continue;
            }

            var slashVfx = CreateJianQiSlashAt(target);
            if (!playedSound)
            {
                PlayIaidoSlashSound(isPerfect);
                playedSound = true;
            }

            slashVfx?.DoSlash();
            slashVfxList.Add(slashVfx);
        }

        await onCounter();

        foreach (var slashVfx in slashVfxList)
        {
            slashVfx?.ForceComplete();
        }
    }

    private static void PlayIaidoSlashSound(bool isPerfect)
    {
        var sounds = isPerfect ? PerfectIaidoSlashSounds : IaidoSlashSounds;
        if (sounds.Length == 0)
        {
            return;
        }

        sounds[Random.Shared.Next(sounds.Length)].Play();
    }

    private static NFeiyapDimensionSlashVfx? CreateJianQiSlashAt(Creature? target, int lineCount = 1)
    {
        if (target == null || lineCount <= 0)
        {
            return null;
        }

        return PlayDimensionSlashTriggerSingle(
            target,
            lineCount,
            SlashAreaMode.TargetExpanded,
            expandDuration: 0.15f,
            keepDuration: 0.3f,
            contractDuration: 0.4f,
            lineFadeIn: 0.08f,
            maxLength: 1f,
            minLength: 1f);
    }

    private static NFeiyapDimensionSlashVfx? PlayDimensionSlashTriggerSingle(
        Creature? target,
        int lineCount,
        SlashAreaMode area,
        Color? color = null,
        float expandDuration = 0.2f,
        float keepDuration = 0.2f,
        float contractDuration = 0.3f,
        float lineFadeIn = 0.15f,
        float maxLength = 0.75f,
        float minLength = 0.45f)
    {
        var slashVfx = CreateSlash(
            target,
            lineCount,
            area,
            color,
            maxLength,
            minLength,
            new NFeiyapDimensionSlashVfx.SlashOptions
            {
                Mode = NFeiyapDimensionSlashVfx.MODE_TRIGGER_SINGLE,
                ExpandSlashDuration = expandDuration,
                KeepSlashDuration = keepDuration,
                ContractSlashDuration = contractDuration,
                LineFadeInDuration = lineFadeIn
            });

        slashVfx?.TriggerSlash();
        return slashVfx;
    }

    private static NFeiyapDimensionSlashVfx? CreateSlash(
        Creature? target,
        int lineCount,
        SlashAreaMode area,
        Color? color,
        float maxLength,
        float minLength,
        NFeiyapDimensionSlashVfx.SlashOptions opts)
    {
        if (target == null || lineCount <= 0)
        {
            return null;
        }

        var room = NCombatRoom.Instance;
        if (room == null)
        {
            return null;
        }

        var creatureNode = room.GetCreatureNode(target);
        if (creatureNode == null)
        {
            return null;
        }

        var length = (maxLength + minLength) * 0.5f;
        if (length < 0.6f)
        {
            length = 0.6f;
        }

        NFeiyapDimensionSlashVfx.GenerateConvergingSlashLines(lineCount, length, 0.1f, out var froms, out var tos);
        var slashVfx = NFeiyapDimensionSlashVfx.Create(creatureNode, opts, froms, tos);
        if (slashVfx == null)
        {
            return null;
        }

        switch (area)
        {
            case SlashAreaMode.FullScreen:
            {
                var vpSize = creatureNode.GetViewport().GetVisibleRect().Size;
                slashVfx.SetSlashArea(Vector2.Zero, vpSize);
                break;
            }
            case SlashAreaMode.TargetExpanded:
            {
                var hitbox = creatureNode.Hitbox;
                var center = hitbox.GlobalPosition + hitbox.Size * 0.5f;
                var areaSize = hitbox.Size * 1.6f;
                var vpSize = creatureNode.GetViewport().GetVisibleRect().Size;
                var minSide = vpSize.X * 0.28f;
                var maxWidth = vpSize.X * 0.8f;
                var maxHeight = vpSize.Y * 0.8f;
                areaSize = new Vector2(
                    Mathf.Clamp(areaSize.X, minSide, maxWidth),
                    Mathf.Clamp(areaSize.Y, minSide, maxHeight));

                const float margin = 50f;
                var topLeft = center - areaSize * 0.5f;
                topLeft = new Vector2(Mathf.Max(topLeft.X, margin), Mathf.Max(topLeft.Y, margin));
                if (topLeft.X + areaSize.X > vpSize.X - margin)
                {
                    topLeft.X = vpSize.X - margin - areaSize.X;
                }

                if (topLeft.Y + areaSize.Y > vpSize.Y - margin)
                {
                    topLeft.Y = vpSize.Y - margin - areaSize.Y;
                }

                slashVfx.SetSlashArea(topLeft, areaSize);
                break;
            }
        }

        if (color.HasValue)
        {
            slashVfx.SetSlashColor(color.Value);
        }

        return slashVfx;
    }
}
