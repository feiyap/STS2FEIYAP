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
    protected bool IsUprightTriggered(Player? player) =>
        player != null
        && !IsReversed
        && FeiyapCombatTracker.Get(player).LastPlayedType == CardType.Skill;

    /// <summary>逆位效果是否满足触发条件（上一张打出的是攻击牌）。</summary>
    protected bool IsReversedTriggered(Player? player) =>
        player != null
        && IsReversed
        && FeiyapCombatTracker.Get(player).LastPlayedType == CardType.Attack;

    /// <summary>当前朝向下，对应塔罗效果是否可触发。</summary>
    protected bool IsTarotEffectTriggered(Player? player) =>
        IsUprightTriggered(player) || IsReversedTriggered(player);

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

    public override bool TryModifyEnergyCostInCombatLate(
        CardModel card,
        decimal originalCost,
        out decimal modifiedCost)
    {
        modifiedCost = originalCost;
        if (card != this || !IsReversed)
        {
            return false;
        }

        modifiedCost = originalCost + 2m;
        return true;
    }
}
