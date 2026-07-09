using Feiyap.Mechanics;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Saves.Runs;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Scaffolding.Content;

namespace Feiyap.Cards.Tarot;

/// <summary>
/// 塔罗牌基类：随机正/逆位，并注册到塔罗牌池。
/// </summary>
public abstract class FeiyapTarotCardBase(
    int baseCost,
    CardType type,
    CardRarity rarity,
    TargetType target,
    bool showInCardLibrary = true)
    : FeiyapCardTemplate(baseCost, type, rarity, target, showInCardLibrary)
{
    private bool _isReversed;
    private bool _orientationInitialized;

    protected override HashSet<CardTag> CanonicalTags => new() { FeiyapCardTags.Tarot };

    protected override bool ShouldGlowGoldInternal =>
        IsUprightTriggered(Owner)
        || FeiyapTarotCmd.HasDualEffect(Owner)
        || FeiyapTarotCmd.HasFreeChoice(Owner);

    protected override bool ShouldGlowRedInternal =>
        IsReversedTriggered(Owner)
        || FeiyapTarotCmd.HasDualEffect(Owner)
        || FeiyapTarotCmd.HasFreeChoice(Owner);

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        FeiyapKeywords.TarotUpright,
        FeiyapKeywords.TarotReversed
    ];

    [SavedProperty]
    public bool IsReversed
    {
        get => _isReversed;
        set
        {
            AssertMutable();
            _isReversed = value;
        }
    }

    [SavedProperty]
    public bool OrientationInitialized
    {
        get => _orientationInitialized;
        set
        {
            AssertMutable();
            _orientationInitialized = value;
        }
    }

    protected FeiyapTarotCardBase(int baseCost, CardType type, TargetType target, bool showInCardLibrary = true)
        : this(baseCost, type, CardRarity.Token, target, showInCardLibrary)
    {
    }

    protected void RegisterTarotFactory(FeiyapTarotCardFactory factory) =>
        FeiyapTarotRegistry.Register(factory);

    /// <summary>正位效果是否满足触发条件（上一张打出的是技能牌）。</summary>
    public static bool IsUprightEffectTriggeredFor(Player? player) =>
        player != null
        && FeiyapCombatTracker.Get(player).LastPlayedType == CardType.Skill;

    /// <summary>逆位效果是否满足触发条件（上一张打出的是攻击牌）。</summary>
    public static bool IsReversedEffectTriggeredFor(Player? player) =>
        player != null
        && FeiyapCombatTracker.Get(player).LastPlayedType == CardType.Attack;

    /// <summary>正位效果是否满足触发条件（上一张打出的是技能牌）。</summary>
    protected bool IsUprightTriggered(Player? player) =>
        IsUprightEffectTriggeredFor(player);

    /// <summary>逆位效果是否满足触发条件（上一张打出的是攻击牌）。</summary>
    protected bool IsReversedTriggered(Player? player) =>
        IsReversedEffectTriggeredFor(player);

    /// <summary>当前朝向下，对应塔罗效果是否可触发。</summary>
    protected bool IsTarotEffectTriggered(Player? player)
    {
        if (FeiyapTarotCmd.HasDualEffect(player) || FeiyapTarotCmd.HasFreeChoice(player))
        {
            return true;
        }

        return IsUprightTriggered(player) || IsReversedTriggered(player);
    }

    /// <summary>按 XXI-世界 等效果执行塔罗正逆位分支。</summary>
    protected async Task RunTarotBranches(
        PlayerChoiceContext choiceContext,
        Func<Task> uprightEffect,
        Func<Task> reversedEffect)
    {
        var player = Owner;
        if (player == null)
        {
            return;
        }

        if (FeiyapTarotCmd.HasDualEffect(player))
        {
            await uprightEffect();
            await reversedEffect();
            return;
        }

        if (FeiyapTarotCmd.HasFreeChoice(player))
        {
            var upright = await FeiyapTarotCmd.ChooseUpright(choiceContext, player, this);
            if (upright)
            {
                await uprightEffect();
            }
            else
            {
                await reversedEffect();
            }

            return;
        }

        if (IsUprightTriggered(player))
        {
            await uprightEffect();
        }
        else if (IsReversedTriggered(player))
        {
            await reversedEffect();
        }
    }

    public void RollOrientation(Rng rng)
    {
        IsReversed = rng.NextBool();
    }

    public override Task BeforeSideTurnStart(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        EnsureOrientationInitialized();
        return Task.CompletedTask;
    }

    protected void EnsureOrientationInitialized()
    {
        if (OrientationInitialized || !IsMutable || Owner == null)
        {
            return;
        }

        RollOrientation(Owner.RunState.Rng.Niche);
        OrientationInitialized = true;
    }
}
