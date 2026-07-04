using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Feiyap.Cards.Quest;
using Feiyap.Characters;
using Feiyap.Mechanics;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Saves.Runs;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
namespace Feiyap.Relics;

/// <summary>
/// 予取 / 予取予夺 共用逻辑。
/// </summary>
public abstract class YuQuBase : ModRelicTemplate
{
    private int _completedQuestFlags;
    private bool _hasMadeInitialQuestSelection;
    private bool _startingQuestCardSelectionInProgress;

    public override RelicRarity Rarity => RelicRarity.Starter;

    public override bool HasUponPickupEffect => true;

    [SavedProperty]
    public int CompletedQuestFlags
    {
        get => _completedQuestFlags;
        set
        {
            AssertMutable();
            _completedQuestFlags = value;
        }
    }

    public bool HasCompletedAllQuests =>
        (CompletedQuestFlags & FeiyapQuestKindExtensions.AllQuestMask) == FeiyapQuestKindExtensions.AllQuestMask;

    /// <summary>本局是否已完成「游戏开始时」的任务牌选择（含选择 0 张的情况）。</summary>
    [SavedProperty]
    public bool HasMadeInitialQuestSelection
    {
        get => _hasMadeInitialQuestSelection;
        set
        {
            AssertMutable();
            _hasMadeInitialQuestSelection = value;
        }
    }

    internal void MarkQuestCompleted(FeiyapQuestKind kind)
    {
        CompletedQuestFlags |= (int)kind;
    }

    public override Task AfterActEntered()
    {
        // EnterAct 在淡入完成后才调用 AfterActEntered，可直接弹选牌。
        if (LocalContext.IsMe(Owner))
        {
            return TrySelectStartingQuestCardAsync();
        }

        return Task.CompletedTask;
    }

    public override Task AfterRoomEntered(AbstractRoom room)
    {
        // 读档不经过 EnterAct，需在房间淡入完成后再补发选牌。
        if (LocalContext.IsMe(Owner))
        {
            TaskHelper.RunSafely(TrySelectStartingQuestCardAfterFadeAsync());
        }

        return Task.CompletedTask;
    }

    private bool HasStartingQuestCard() =>
        Owner.Deck.Cards.Any(c => c is FeiyapQuestCardBase);

    /// <summary>是否仍需弹出「游戏开始时」的任务牌选择界面。</summary>
    private bool NeedsInitialQuestSelection() =>
        !HasMadeInitialQuestSelection
        && CompletedQuestFlags == 0
        && !HasStartingQuestCard();

    private async Task TrySelectStartingQuestCardAfterFadeAsync()
    {
        var transition = NGame.Instance?.Transition;
        while (transition != null && transition.InTransition)
        {
            await Task.Delay(16);
        }

        await TrySelectStartingQuestCardAsync();
    }

    private async Task TrySelectStartingQuestCardAsync()
    {
        if (!NeedsInitialQuestSelection() || _startingQuestCardSelectionInProgress)
        {
            return;
        }

        _startingQuestCardSelectionInProgress = true;
        try
        {
            await SelectStartingQuestCardCoreAsync();
        }
        finally
        {
            _startingQuestCardSelectionInProgress = false;
        }
    }

    private async Task SelectStartingQuestCardCoreAsync()
    {
        if (!NeedsInitialQuestSelection())
        {
            return;
        }

        var choices = BuildAvailableQuestChoices();
        if (choices.Count == 0)
        {
            HasMadeInitialQuestSelection = true;
            return;
        }

        var prompt = new LocString("relics", "FEIYAP_RELIC_YU_QU.selectionPrompt");
        var prefs = new CardSelectorPrefs(prompt, 0, choices.Count)
        {
            Cancelable = false,
            RequireManualConfirmation = true
        };

        var selected = await CardSelectCmd.FromSimpleGrid(
            new BlockingPlayerChoiceContext(),
            choices,
            Owner,
            prefs);
        if (selected.Any())
        {
            CardCmd.PreviewCardPileAdd(await CardPileCmd.Add(selected, PileType.Deck));
        }

        HasMadeInitialQuestSelection = true;
    }

    private List<CardModel> BuildAvailableQuestChoices()
    {
        var choices = new List<CardModel>();
        if ((CompletedQuestFlags & (int)FeiyapQuestKind.MiWang) == 0)
        {
            choices.Add(Owner.RunState.CreateCard<MiWang>(Owner));
        }

        if ((CompletedQuestFlags & (int)FeiyapQuestKind.KuXiu) == 0)
        {
            choices.Add(Owner.RunState.CreateCard<KuXiu>(Owner));
        }

        if ((CompletedQuestFlags & (int)FeiyapQuestKind.FaWei) == 0)
        {
            choices.Add(Owner.RunState.CreateCard<FaWei>(Owner));
        }

        return choices;
    }

}

/// <summary>
/// 予取：游戏开始时选择转职任务牌。
/// </summary>
[RegisterRelic(typeof(FeiyapRelicPool))]
[RegisterCharacterStarterRelic(typeof(FeiyapCharacter))]
[RegisterTouchOfOrobasRefinement(typeof(YuQuYuDuo))]
public sealed class YuQu : YuQuBase
{
    public override RelicAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/relics/{nameof(YuQu)}.png",
        IconOutlinePath: $"{Entry.ResPath}/images/relics/{nameof(YuQu)}.png",
        BigIconPath: $"{Entry.ResPath}/images/relics/{nameof(YuQu)}.png");
}

/// <summary>
/// 予取予夺：任务奖励遗物升级。
/// </summary>
[RegisterRelic(typeof(FeiyapRelicPool))]
public sealed class YuQuYuDuo : YuQuBase
{
    public override RelicAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/relics/{nameof(YuQuYuDuo)}.png",
        IconOutlinePath: $"{Entry.ResPath}/images/relics/{nameof(YuQuYuDuo)}.png",
        BigIconPath: $"{Entry.ResPath}/images/relics/{nameof(YuQuYuDuo)}.png");
}
