using Feiyap.Audio;
using Feiyap.Cards;
using Feiyap.Cards.Basic;
using Feiyap.Cards.Rare;
using Feiyap.Cards.Uncommon;
using Feiyap.Powers;
using Feiyap.Vfx;
using Godot;
using MegaCrit.Sts2.Core.Audio.Debug;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using STS2RitsuLib.Scaffolding.Characters;

namespace Feiyap.Mechanics;

/// <summary>
/// 多维斩击特效：攻击命中与居合反击共用。
/// </summary>
public static class FeiyapSlashCmd
{
    /// <summary>默认剑气色（无绯神乐/斋时雨时）。</summary>
    public static readonly Color DefaultSlashColor = Color.Color8(255, 81, 49, 255);

    /// <summary>绯神乐：红色。</summary>
    public static readonly Color ScarletSlashColor = new(1f, 0.16f, 0.2f, 1f);

    /// <summary>斋时雨：青色。</summary>
    public static readonly Color CyanSlashColor = new(0.18f, 0.88f, 0.95f, 1f);

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

    /// <summary>是否为应挂普通斩击命中特效的绯夜攻击牌。</summary>
    public static bool ShouldAttachCardHitSlash(CardModel card)
    {
        if (card is not FeiyapCardTemplate || card.Type != CardType.Attack)
        {
            return false;
        }

        // 剑鞘打击、分铜锁：保持默认命中；枯山水改用全屏斩屏。
        return card is not JianQiaoDaJi
            and not FeiyapUncommon6
            and not FeiyapRare4;
    }

    /// <summary>挂上原版斩击命中特效与音效。</summary>
    public static AttackCommand AttachCardHitSlash(AttackCommand command) =>
        command
            .WithHitFx(VfxCmd.slashPath, null, TmpSfx.slashAttack)
            .SpawningHitVfxOnEachCreature();

    /// <summary>居合单体反击：先播放斩击，再执行反击伤害。</summary>
    public static async Task PlayIaidoCounterSlash(
        Creature? target,
        Func<Task> onCounter,
        bool isPerfect = false,
        Creature? owner = null)
    {
        if (target == null)
        {
            await onCounter();
            return;
        }

        var slashVfx = CreateJianQiSlashAt(target, ResolveSlashStyle(owner));
        PlayIaidoSlashSound(isPerfect);
        slashVfx?.DoSlash();
        await onCounter();
        slashVfx?.ForceComplete();
    }

    /// <summary>居合群体反击：先对每个目标播放斩击，再执行反击伤害。</summary>
    public static async Task PlayIaidoCounterSlashAll(
        IReadOnlyList<Creature> targets,
        Func<Task> onCounter,
        bool isPerfect = false,
        Creature? owner = null)
    {
        var style = ResolveSlashStyle(owner);
        var slashVfxList = new List<NFeiyapDimensionSlashVfx?>();
        var playedSound = false;
        foreach (var target in targets)
        {
            if (!target.IsAlive)
            {
                continue;
            }

            var slashVfx = CreateJianQiSlashAt(target, style);
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

    /// <summary>根据绯神乐 / 斋时雨决定斩击着色。</summary>
    public static SlashColorStyle ResolveSlashStyle(Creature? owner)
    {
        if (owner == null)
        {
            return SlashColorStyle.Single(DefaultSlashColor);
        }

        var hasScarlet = owner.FindPower<FeiyapScarletKaguraPower>() != null;
        var hasRain = owner.FindPower<FeiyapIaidoRainPower>() != null;
        if (hasScarlet && hasRain)
        {
            return SlashColorStyle.Gradient(ScarletSlashColor, CyanSlashColor);
        }

        if (hasScarlet)
        {
            return SlashColorStyle.Single(ScarletSlashColor);
        }

        if (hasRain)
        {
            return SlashColorStyle.Single(CyanSlashColor);
        }

        return SlashColorStyle.Single(DefaultSlashColor);
    }

    public readonly record struct SlashColorStyle(Color Primary, Color? Secondary)
    {
        public static SlashColorStyle Single(Color color) => new(color, null);

        public static SlashColorStyle Gradient(Color a, Color b) => new(a, b);

        public void ApplyTo(NFeiyapDimensionSlashVfx slashVfx)
        {
            if (Secondary.HasValue)
            {
                slashVfx.SetSlashGradient(Primary, Secondary.Value);
            }
            else
            {
                slashVfx.SetSlashColor(Primary);
            }
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

    private static NFeiyapDimensionSlashVfx? CreateJianQiSlashAt(Creature? target, SlashColorStyle style, int lineCount = 1)
    {
        if (target == null || lineCount <= 0)
        {
            return null;
        }

        return PlayDimensionSlashTriggerSingle(
            target,
            lineCount,
            SlashAreaMode.TargetExpanded,
            style,
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
        SlashColorStyle style,
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
            style,
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
        SlashColorStyle style,
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

        style.ApplyTo(slashVfx);
        return slashVfx;
    }
}
