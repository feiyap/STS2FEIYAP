using System.Linq;
using Feiyap.Characters;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Feiyap.Cards.Common;

/// <summary>
/// 缩地：将手牌中的一张牌放到抽牌堆底部，从抽牌堆中选择 1 张相同类型的牌放入手牌。
/// </summary>
[RegisterCard(typeof(FeiyapCardPool))]
public sealed class FeiyapCommon17 : FeiyapCardTemplate
{
    private static readonly LocString DrawSelectionPrompt = new("cards", "FEIYAP_CARD_FEIYAP_COMMON17.drawSelectionPrompt");

    public FeiyapCommon17()
        : base(1, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var handCard = (await CardSelectCmd.FromHand(
            choiceContext,
            Owner,
            new CardSelectorPrefs(SelectionScreenPrompt, 1),
            null,
            this)).FirstOrDefault();

        if (handCard == null)
        {
            return;
        }

        var cardType = handCard.Type;
        await CardPileCmd.Add(handCard, PileType.Draw, CardPilePosition.Bottom);

        var drawPile = PileType.Draw.GetPile(Owner);
        var selected = await CardSelectCmd.FromCombatPile(
            choiceContext,
            drawPile,
            Owner,
            new CardSelectorPrefs(DrawSelectionPrompt, 1),
            c => c.Type == cardType);

        var picked = selected.FirstOrDefault();
        if (picked != null)
        {
            await CardPileCmd.Add(picked, PileType.Hand);
        }
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
