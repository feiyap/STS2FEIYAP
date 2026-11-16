using System;
using System.Collections.Generic;
using Feiyap.Cards;
using Feiyap.Cards.Ancients;
using Feiyap.Cards.Basic;
using Feiyap.Cards.Common;
using Feiyap.Cards.Rare;
using Feiyap.Cards.Uncommon;
using Feiyap.Vfx;
using Godot;
using MegaCrit.Sts2.Core.Audio.Debug;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;

namespace Feiyap.Mechanics;

/// <summary>
/// 绯夜攻击牌命中特效配置：按卡牌主题挑选原版 / 自定义特效。
/// </summary>
public static class FeiyapAttackHitVfx
{
    public enum HitKind
    {
        /// <summary>不挂命中特效（如枯山水已有全屏斩屏）。</summary>
        None,

        /// <summary>原版场景特效路径。</summary>
        Vanilla,

        /// <summary>自定义多维斩击。</summary>
        DimensionSlash
    }

    public readonly record struct Profile(
        HitKind Kind,
        string? VfxPath = null,
        string? Sfx = null,
        string? TmpSfx = null,
        bool SpawnOnEachCreature = true,
        bool SpawnAtBase = false,
        Color? DimensionColor = null,
        Color? DimensionColorB = null,
        int DimensionLines = 1);

    private static readonly Color MushinSilver = new(0.85f, 0.9f, 0.95f, 1f);
    private static readonly Color DeathPurple = new(0.55f, 0.2f, 0.85f, 1f);
    private static readonly Color DeathBlack = new(0.15f, 0.05f, 0.2f, 1f);

    private static readonly Dictionary<Type, Profile> Profiles = new()
    {
        // ── 基础 ──
        // 打击：标准斩击
        [typeof(FeiyapStrike)] = Vanilla(VfxCmd.slashPath, TmpSfx.slashAttack),
        // 剑鞘打击：钝击（鞘打）
        [typeof(JianQiaoDaJi)] = Vanilla(VfxCmd.bluntPath, TmpSfx.bluntAttack),

        // ── 普通 ──
        // 晚樱：花瓣般斩击
        [typeof(FeiyapCommon1)] = Vanilla(VfxCmd.slashPath, TmpSfx.slashAttack),
        // 燕返：迅捷连斩
        [typeof(FeiyapCommon2)] = Vanilla(VfxCmd.slashPath, TmpSfx.slashAttack),
        // 松风：风之斩击
        [typeof(FeiyapCommon3)] = Vanilla(VfxCmd.slashPath, TmpSfx.slashAttack),
        // 残月：弧月斩
        [typeof(FeiyapCommon4)] = Vanilla(VfxCmd.slashPath, TmpSfx.slashAttack),
        // 朔风切：连段斩击
        [typeof(FeiyapCommon5)] = Vanilla(VfxCmd.slashPath, TmpSfx.slashAttack),
        // 牙突：突刺
        [typeof(FeiyapCommon6)] = Vanilla(VfxCmd.dramaticStabPath, TmpSfx.slashAttack),
        // IV-皇帝：威压重击
        [typeof(FeiyapCommon7)] = Vanilla(VfxCmd.heavyBluntPath, TmpSfx.heavyAttack, spawnAtBase: true),
        // VIII-正义：公正之刃
        [typeof(FeiyapCommon8)] = Vanilla(VfxCmd.slashPath, TmpSfx.slashAttack),
        // XVI-塔：崩毁冲击
        [typeof(FeiyapCommon9)] = Vanilla(VfxCmd.rockShatterPath, TmpSfx.heavyAttack),

        // ── 罕见 ──
        // 空切：刺穿虚空
        [typeof(FeiyapUncommon1)] = Vanilla(VfxCmd.dramaticStabPath, TmpSfx.slashAttack),
        // 逆鳞斩：血腥连斩
        [typeof(FeiyapUncommon2)] = Vanilla(VfxCmd.bloodyImpactPath, TmpSfx.slashAttack),
        // 迷雾斩：斩击
        [typeof(FeiyapUncommon3)] = Vanilla(VfxCmd.slashPath, TmpSfx.slashAttack),
        // 逆袈裟：大横斩
        [typeof(FeiyapUncommon4)] = Vanilla(VfxCmd.giantHorizontalSlashPath, TmpSfx.slashAttack),
        // 血溅五步：血溅
        [typeof(FeiyapUncommon5)] = Vanilla(VfxCmd.bloodyImpactPath, TmpSfx.heavyAttack),
        // 分铜锁：锁链束缚
        [typeof(FeiyapUncommon6)] = Vanilla(VfxCmd.chainPath, TmpSfx.bluntAttack),
        // 无明：斩击
        [typeof(FeiyapUncommon7)] = Vanilla(VfxCmd.slashPath, TmpSfx.slashAttack),
        // 秘剑：戏剧性突刺
        [typeof(FeiyapUncommon8)] = Vanilla(VfxCmd.dramaticStabPath, TmpSfx.slashAttack),
        // XI-力量：重击群攻
        [typeof(FeiyapUncommon9)] = Vanilla(VfxCmd.heavyBluntPath, TmpSfx.heavyAttack, spawnAtBase: true),

        // ── 稀有 ──
        // 天下五剑：五段斩击（每段一剑）
        [typeof(FeiyapRare1)] = Vanilla(VfxCmd.slashPath, TmpSfx.slashAttack),
        // 无想斩：银白空间斩（自定义）
        [typeof(FeiyapRare2)] = Dimension(MushinSilver, null, lines: 1, TmpSfx.slashAttack),
        // 天人五衰：衰败尖叫冲击
        [typeof(FeiyapRare3)] = Vanilla(VfxCmd.screamVfx, TmpSfx.heavyAttack),
        // 枯山水：全屏斩屏，不挂普通命中
        [typeof(FeiyapRare4)] = new Profile(HitKind.None),
        // 神座屠：横扫大斩（屠神）
        [typeof(FeiyapRare5)] = Vanilla(VfxCmd.giantHorizontalSlashPath, TmpSfx.heavyAttack),
        // VII-战车：冲撞击碎
        [typeof(FeiyapRare6)] = Vanilla(VfxCmd.rockShatterPath, TmpSfx.heavyAttack),
        // XIII-死神：紫黑空间斩（自定义）
        [typeof(FeiyapRare7)] = Dimension(DeathPurple, DeathBlack, lines: 2, TmpSfx.heavyAttack),
        // XIX-太阳：星辉冲击
        [typeof(FeiyapRare8)] = Vanilla(VfxCmd.starryImpactVfx, TmpSfx.slashAttack),

        // ── 先古 ──
        // 渴血症：吸血冲击
        [typeof(KeXueZheng)] = Vanilla(VfxCmd.bloodyImpactPath, TmpSfx.slashAttack),
    };

    /// <summary>若卡牌有配置则挂上命中特效。</summary>
    public static bool TryAttach(AttackCommand command, CardModel card)
    {
        if (card is not FeiyapCardTemplate)
        {
            return false;
        }

        if (!Profiles.TryGetValue(card.GetType(), out var profile))
        {
            // 未登记的绯夜攻击牌：回退标准斩击
            if (card.Type == CardType.Attack)
            {
                profile = Vanilla(VfxCmd.slashPath, TmpSfx.slashAttack);
            }
            else
            {
                return false;
            }
        }

        return Attach(command, profile);
    }

    private static bool Attach(AttackCommand command, Profile profile)
    {
        switch (profile.Kind)
        {
            case HitKind.None:
                return false;

            case HitKind.Vanilla:
            {
                if (string.IsNullOrEmpty(profile.VfxPath))
                {
                    return false;
                }

                command.WithHitFx(profile.VfxPath, profile.Sfx, profile.TmpSfx);
                if (profile.SpawnOnEachCreature)
                {
                    command.SpawningHitVfxOnEachCreature();
                }

                if (profile.SpawnAtBase)
                {
                    command.WithHitVfxSpawnedAtBase();
                }

                return true;
            }

            case HitKind.DimensionSlash:
            {
                var color = profile.DimensionColor ?? FeiyapSlashCmd.DefaultSlashColor;
                var colorB = profile.DimensionColorB;
                var lines = Math.Max(1, profile.DimensionLines);
                var tmpSfx = profile.TmpSfx;
                command.WithHitVfxNode(target =>
                    FeiyapSlashCmd.CreateCardDimensionHitNode(target, color, colorB, lines, tmpSfx));
                return true;
            }

            default:
                return false;
        }
    }

    private static Profile Vanilla(
        string vfx,
        string tmpSfx,
        bool spawnOnEach = true,
        bool spawnAtBase = false) =>
        new(HitKind.Vanilla, vfx, Sfx: null, TmpSfx: tmpSfx,
            SpawnOnEachCreature: spawnOnEach, SpawnAtBase: spawnAtBase);

    private static Profile Dimension(
        Color colorA,
        Color? colorB,
        int lines,
        string tmpSfx) =>
        new(HitKind.DimensionSlash,
            DimensionColor: colorA,
            DimensionColorB: colorB,
            DimensionLines: lines,
            TmpSfx: tmpSfx);
}
