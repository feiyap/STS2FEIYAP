using System.Threading.Tasks;
using Feiyap.Mechanics;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Saves.Runs;
using STS2RitsuLib.Scaffolding.Content;

namespace Feiyap.Cards.Quest;

// 任务牌共用占位卡图，避免回退到不存在的 card_atlas 条目。

/// <summary>
/// 绯夜氏转职任务牌基类。
/// </summary>
public abstract class FeiyapQuestCardBase : FeiyapCardTemplate
{
    private int _progress;

    protected abstract FeiyapQuestKind QuestKind { get; }

    protected abstract int QuestGoal { get; }

    protected abstract Task GrantReward(PlayerChoiceContext choiceContext);

    public override int MaxUpgradeLevel => 0;

    [SavedProperty]
    public int Progress
    {
        get => _progress;
        set
        {
            AssertMutable();
            _progress = value;
            DynamicVars["Progress"].BaseValue = _progress;
            ApplyPlayabilityState();
        }
    }

    public bool IsComplete => Progress >= QuestGoal;

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Eternal,
        CardKeyword.Unplayable
    ];

    protected override bool IsPlayable => IsComplete;

    protected override bool ShouldGlowGoldInternal => IsComplete;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new IntVar("Progress", 0),
        new IntVar("Goal", QuestGoal)
    ];

    public override CardAssetProfile AssetProfile => FeiyapCardAssets.For("FeiyapQuest");

    protected FeiyapQuestCardBase()
        : base(0, CardType.Quest, CardRarity.Quest, TargetType.Self, showInCardLibrary: false)
    {
    }

    /// <summary>牌库中的任务牌本体；战斗内同名卡为克隆体并通过 <see cref="DeckVersion"/> 关联。</summary>
    internal FeiyapQuestCardBase CanonicalQuest =>
        DeckVersion is FeiyapQuestCardBase deckQuest ? deckQuest : this;

    internal bool IsCombatClone => DeckVersion is FeiyapQuestCardBase;

    public void AddProgress(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        var canonical = CanonicalQuest;
        if (canonical != this)
        {
            canonical.AddProgress(amount);
            return;
        }

        if (IsComplete)
        {
            return;
        }

        Progress = Math.Min(QuestGoal, Progress + amount);

        SyncProgressToCombatClones();
        FeiyapQuestCardVisuals.RefreshQuestProgress(this);
    }

    internal void SyncProgressToCombatClones()
    {
        if (Owner?.PlayerCombatState == null)
        {
            return;
        }

        foreach (var card in Owner.PlayerCombatState.AllCards)
        {
            if (card.DeckVersion == this && card is FeiyapQuestCardBase clone)
            {
                clone.CopyProgressFrom(this);
            }
        }
    }

    private void CopyProgressFrom(FeiyapQuestCardBase source)
    {
        Progress = source.Progress;
    }

    /// <summary>任务完成后移除「无法打出」，否则 <see cref="CardKeyword.Unplayable"/> 会一直阻止出牌。</summary>
    private void ApplyPlayabilityState()
    {
        if (!IsComplete || !Keywords.Contains(CardKeyword.Unplayable))
        {
            return;
        }

        CardCmd.RemoveKeyword(this, CardKeyword.Unplayable);
    }

    public override void AfterCreated()
    {
        DynamicVars["Goal"].BaseValue = QuestGoal;
        DynamicVars["Progress"].BaseValue = Progress;
        ApplyPlayabilityState();
    }

    protected override void AfterDeserialized()
    {
        base.AfterDeserialized();
        ApplyPlayabilityState();
    }

    protected override PileType GetResultPileTypeForCardPlay() => PileType.None;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var deckQuest = CanonicalQuest;
        await CardPileCmd.RemoveFromDeck(deckQuest);
        PlayerCmd.CompleteQuest(deckQuest);
        await GrantReward(choiceContext);
        FeiyapQuestRewards.MarkQuestCompleted(Owner!, QuestKind);
        await FeiyapQuestRewards.TryGrantLaplaceDemon(Owner!);
    }
}
