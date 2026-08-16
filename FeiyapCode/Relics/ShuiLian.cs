using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Feiyap.Cards.Quest;
using Feiyap.Characters;
using Feiyap.Mechanics;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
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
/// 睡莲 / 莲心守月 共用逻辑。
/// </summary>
public abstract class ShuiLianBase : ModRelicTemplate
{
    private int _completedQuestFlags;
    private bool _hasMadeInitialQuestSelection;
    private Task? _startingQuestCardSelectionTask;

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
        // 联机时所有客户端都必须执行同一套选牌/状态更新逻辑；CardSelectCmd 会自行同步玩家选择。
        // 幕间任务进度仅在多人模式下发放。
        if (Owner.RunState.CurrentActIndex > 0 && Owner.RunState.Players.Count > 1)
        {
            FeiyapQuestProgress.GrantActEndProgress(Owner);
        }

        return TrySelectStartingQuestCardAsync();
    }

    public override Task AfterRoomEntered(AbstractRoom room)
    {
        // 读档不经过 EnterAct，在此补发选牌；但不能 await，否则会阻塞 FadeIn（进涅奥前黑屏）。
        // 联机时所有客户端都必须启动同一任务，CardSelectCmd 会同步玩家选择。
        TaskHelper.RunSafely(TrySelectStartingQuestCardAfterFadeAsync());
        return Task.CompletedTask;
    }

    private bool HasStartingQuestCard() =>
        Owner.Deck.Cards.Any(c => c is FeiyapQuestCardBase);

    /// <summary>
    /// 从局内其它状态回填任务进度。欧罗巴斯之触将睡莲替换为莲心守月时不会复制 SavedProperty，
    /// 读档后若仅依赖遗物字段会误判为「尚未选过任务牌」。
    /// </summary>
    private void SyncQuestProgressFromRunState()
    {
        if (Owner == null)
        {
            return;
        }

        if (HasStartingQuestCard()
            || this is LianXinShouYue
            || Owner.Relics.Any(static r => r is LaplaceDemon))
        {
            HasMadeInitialQuestSelection = true;
        }

        if (Owner.Relics.Any(static r => r is InvestigatorBase))
        {
            CompletedQuestFlags |= (int)FeiyapQuestKind.MiWang;
        }

        if (Owner.Relics.Any(static r => r is SwordSaintBase))
        {
            CompletedQuestFlags |= (int)FeiyapQuestKind.KuXiu;
        }

        if (Owner.Relics.Any(static r => r is MerryWitchBase))
        {
            CompletedQuestFlags |= (int)FeiyapQuestKind.FaWei;
        }

        if (CompletedQuestFlags != 0)
        {
            HasMadeInitialQuestSelection = true;
        }
    }

    /// <summary>是否仍需弹出「游戏开始时」的任务牌选择界面。</summary>
    private bool NeedsInitialQuestSelection()
    {
        SyncQuestProgressFromRunState();
        return !HasMadeInitialQuestSelection
            && !HasStartingQuestCard()
            && !HasCompletedAllQuests;
    }

    private async Task TrySelectStartingQuestCardAfterFadeAsync()
    {
        var transition = NGame.Instance?.Transition;
        while (transition != null && transition.InTransition)
        {
            await Task.Delay(16);
        }

        await TrySelectStartingQuestCardAsync();
    }

    private Task TrySelectStartingQuestCardAsync()
    {
        if (!NeedsInitialQuestSelection())
        {
            return Task.CompletedTask;
        }

        return _startingQuestCardSelectionTask ??= RunStartingQuestCardSelectionAsync();
    }

    private async Task RunStartingQuestCardSelectionAsync()
    {
        try
        {
            await SelectStartingQuestCardCoreAsync();
        }
        finally
        {
            _startingQuestCardSelectionTask = null;
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

        var prompt = new LocString("relics", "FEIYAP_RELIC_SHUI_LIAN.selectionPrompt");
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
/// 睡莲：游戏开始时选择转职任务牌。
/// </summary>
[RegisterRelic(typeof(FeiyapRelicPool))]
[RegisterCharacterStarterRelic(typeof(FeiyapCharacter))]
[RegisterTouchOfOrobasRefinement(typeof(LianXinShouYue))]
public sealed class ShuiLian : ShuiLianBase
{
    public override RelicAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/relics/{nameof(ShuiLian)}.png",
        IconOutlinePath: $"{Entry.ResPath}/images/relics/{nameof(ShuiLian)}.png",
        BigIconPath: $"{Entry.ResPath}/images/relics/{nameof(ShuiLian)}.png");
}

/// <summary>
/// 莲心守月：任务奖励遗物升级。
/// </summary>
[RegisterRelic(typeof(FeiyapRelicPool))]
public sealed class LianXinShouYue : ShuiLianBase
{
    public override RelicAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/relics/{nameof(LianXinShouYue)}.png",
        IconOutlinePath: $"{Entry.ResPath}/images/relics/{nameof(LianXinShouYue)}.png",
        BigIconPath: $"{Entry.ResPath}/images/relics/{nameof(LianXinShouYue)}.png");

    public override async Task AfterObtained()
    {
        await FeiyapQuestRewards.UpgradeExistingQuestRelics(Owner);
    }
}
