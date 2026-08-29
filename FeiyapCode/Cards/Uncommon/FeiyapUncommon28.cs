using System.Linq;
using Feiyap.Characters;
using Feiyap.Cards.Tarot;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Feiyap.Cards.Uncommon;

/// <summary>
/// XII-倒吊人：正位从队友手牌取牌，逆位在队友手牌生成复制。
/// </summary>
[RegisterCard(typeof(FeiyapCardPool))]
public sealed class FeiyapUncommon28 : FeiyapTarotCardBase
{
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Exhaust,
        ..base.CanonicalKeywords
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CardsVar(1)
    ];

    public FeiyapUncommon28()
        : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
        RegisterTarotFactory(player => player.RunState.CreateCard<FeiyapUncommon28>(player));
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        EnsureOrientationInitialized();

        await RunTarotBranches(
            choiceContext,
            () => TakeFromTeammateHands(choiceContext),
            () => AddCopyToTeammateHands(choiceContext));
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Cards.UpgradeValueBy(1m);
    }

    private async Task TakeFromTeammateHands(PlayerChoiceContext choiceContext)
    {
        if (CombatState is not CombatState state)
        {
            return;
        }

        var prefs = new CardSelectorPrefs(SelectionScreenPrompt, 0, 1)
        {
            Cancelable = true,
            RequireManualConfirmation = true
        };

        var maxPick = DynamicVars.Cards.IntValue;
        for (var i = 0; i < maxPick; i++)
        {
            var candidates = state.GetTeammatesOf(Owner.Creature)
                .Where(c => c.IsPlayer && c != Owner.Creature && c.Player != null)
                .SelectMany(c => c.Player!.PlayerCombatState?.Hand.Cards ?? [])
                .ToList();

            if (candidates.Count == 0)
            {
                break;
            }

            var selected = await CardSelectCmd.FromSimpleGrid(
                choiceContext,
                candidates,
                Owner,
                prefs);

            var card = selected.FirstOrDefault();
            if (card == null)
            {
                break;
            }

            // 手牌 UI 不监听 CardPile.CardRemoved。必须先拆掉本地视觉节点，
            // 再换主人；否则队友客户端会留下幽灵卡面。
            DetachLocalHandVisual(card);

            // Owner 不可直接从 A 改到 B，须先清空再登记到 CombatState
            card.RemoveFromCurrentPile();
            if (state.ContainsCard(card))
            {
                state.RemoveCard(card);
            }

            state.AddCard(card, Owner);
            await CardPileCmd.Add(card, PileType.Hand);
        }
    }

    private async Task AddCopyToTeammateHands(PlayerChoiceContext choiceContext)
    {
        if (CombatState is not CombatState state)
        {
            return;
        }

        foreach (var teammate in state.GetTeammatesOf(Owner.Creature).Where(c => c.IsPlayer && c != Owner.Creature))
        {
            if (teammate.Player == null)
            {
                continue;
            }

            // 战斗内生成牌必须走 CombatState.CreateCard，否则打出时 AddDuringManualCardPlay 会因
            // 不在 CombatState 中抛错，表现为卡牌卡在屏幕上。
            var copy = state.CreateCard<FeiyapUncommon28>(teammate.Player);
            await CardPileCmd.AddGeneratedCardToCombat(copy, PileType.Hand, teammate.Player);
        }
    }

    /// <summary>
    /// 从本机手牌 UI 拆掉卡面，避免换主人后旧节点残留。
    /// </summary>
    private static void DetachLocalHandVisual(CardModel card)
    {
        if (!LocalContext.IsMe(card.Owner))
        {
            return;
        }

        var hand = NCombatRoom.Instance?.Ui?.Hand;
        if (hand?.GetCardHolder(card) == null)
        {
            return;
        }

        hand.Remove(card);
    }
}
