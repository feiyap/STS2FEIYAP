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
public abstract class FeiyapQuestCardBase : ModCardTemplate
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
        }
    }

    public bool IsComplete => Progress >= QuestGoal;

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Eternal,
        CardKeyword.Unplayable
    ];

    protected override bool IsPlayable => IsComplete;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new IntVar("Progress", 0),
        new IntVar("Goal", QuestGoal)
    ];

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/FeiyapQuest.png");

    protected FeiyapQuestCardBase()
        : base(0, CardType.Quest, CardRarity.Quest, TargetType.Self, showInCardLibrary: false)
    {
    }

    public void AddProgress(int amount)
    {
        if (amount <= 0 || IsComplete)
        {
            return;
        }

        Progress = Math.Min(QuestGoal, Progress + amount);
        if (IsComplete)
        {
            MarkQuestReadyToPlay();
        }
    }

    protected void MarkQuestReadyToPlay()
    {
        if (Keywords.Contains(CardKeyword.Unplayable))
        {
            CardCmd.RemoveKeyword(this, CardKeyword.Unplayable);
        }
    }

    public override void AfterCreated()
    {
        DynamicVars["Goal"].BaseValue = QuestGoal;
        DynamicVars["Progress"].BaseValue = Progress;
        if (IsComplete)
        {
            MarkQuestReadyToPlay();
        }
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CardPileCmd.RemoveFromDeck(this);
        PlayerCmd.CompleteQuest(this);
        await GrantReward(choiceContext);
        FeiyapQuestRewards.MarkQuestCompleted(Owner, QuestKind);
        await FeiyapQuestRewards.TryGrantLaplaceDemon(Owner);
    }
}
