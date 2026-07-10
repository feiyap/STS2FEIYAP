using Godot;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Screens;
using MegaCrit.Sts2.Core.Nodes.Screens.Capstones;
using MegaCrit.Sts2.Core.TestSupport;
using Feiyap.Cards.Quest;

namespace Feiyap.Mechanics;

/// <summary>
/// 任务牌进度变更后刷新可见卡牌 UI。
/// </summary>
internal static class FeiyapQuestCardVisuals
{
    internal static void RefreshCardVisuals(CardModel card)
    {
        if (TestMode.IsOn)
        {
            return;
        }

        RefreshCard(card);
    }

    internal static void RefreshQuestProgress(FeiyapQuestCardBase canonical)
    {
        if (TestMode.IsOn)
        {
            return;
        }

        RefreshCard(canonical);

        if (canonical.Owner?.PlayerCombatState == null)
        {
            return;
        }

        foreach (var card in canonical.Owner.PlayerCombatState.AllCards)
        {
            if (card.DeckVersion == canonical)
            {
                RefreshCard(card);
            }
        }
    }

    private static void RefreshCard(CardModel card)
    {
        var pileType = card.Pile?.Type ?? PileType.Deck;

        NCard.FindOnTable(card)?.UpdateVisuals(pileType, CardPreviewMode.Normal);
        NCombatRoom.Instance?.Ui?.Hand?.GetCard(card)
            ?.UpdateVisuals(PileType.Hand, CardPreviewMode.Normal);

        if (NCapstoneContainer.Instance?.CurrentCapstoneScreen is not Node capstoneScreen)
        {
            return;
        }

        var displayPile = capstoneScreen is NCardPileScreen pileScreen
            ? pileScreen.Pile.Type
            : PileType.Deck;

        capstoneScreen.GetNodeOrNull<NCardGrid>("CardGrid")
            ?.GetCardNode(card)
            ?.UpdateVisuals(displayPile, CardPreviewMode.Normal);
    }
}
