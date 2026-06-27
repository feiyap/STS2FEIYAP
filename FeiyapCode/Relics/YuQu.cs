using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Feiyap.Cards.Quest;
using Feiyap.Characters;
using Feiyap.Mechanics;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
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
        if (HasStartingQuestCard() || _startingQuestCardSelectionInProgress)
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
        if (HasStartingQuestCard())
        {
            return;
        }

        var prompt = new LocString("relics", "FEIYAP_RELIC_YU_QU.selectionPrompt");
        var prefs = new CardSelectorPrefs(prompt, 0, 4)
        {
            Cancelable = false,
            RequireManualConfirmation = true
        };

        var choices = new List<CardModel>
        {
            Owner.RunState.CreateCard<MiWang>(Owner),
            Owner.RunState.CreateCard<KuXiu>(Owner),
            Owner.RunState.CreateCard<LiuLang>(Owner),
            Owner.RunState.CreateCard<FaWei>(Owner)
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
    }

    public override Task BeforeSideTurnStart(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        if (participants.Contains(Owner.Creature) && side == Owner.Creature.Side)
        {
            FeiyapCombatTracker.Get(Owner).ConstellationPlayedThisTurn = false;
            FeiyapPerfectIaidoCmd.OnPlayerTurnStart(Owner);
        }

        return Task.CompletedTask;
    }

    public override Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner != Owner)
        {
            return Task.CompletedTask;
        }

        var tracker = FeiyapCombatTracker.Get(Owner);
        FeiyapQuestProgress.RecordCardPlayed(Owner, cardPlay.Card.Type);

        if (FeiyapCardTags.HasConstellation(cardPlay.Card))
        {
            tracker.ConstellationPlayedThisTurn = true;
        }

        return Task.CompletedTask;
    }

    public override Task AfterCombatEnd(CombatRoom _)
    {
        FeiyapCombatTracker.ClearCombatState(Owner);
        return Task.CompletedTask;
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
