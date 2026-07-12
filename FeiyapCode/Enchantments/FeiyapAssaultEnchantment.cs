using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Feiyap.Enchantments;

/// <summary>
/// 强袭：打出其他攻击牌时，自动打出这张牌。
/// </summary>
[RegisterEnchantment]
public sealed class FeiyapAssaultEnchantment : ModEnchantmentTemplate
{
    public override bool HasExtraCardText => true;

    public override bool CanEnchantCardType(CardType cardType) => cardType == CardType.Attack;

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.IsAutoPlay)
        {
            return;
        }

        if (cardPlay.Card.Owner != Card.Owner)
        {
            return;
        }

        if (cardPlay.Card == Card)
        {
            return;
        }

        if (cardPlay.Card.Type != CardType.Attack)
        {
            return;
        }

        if (Card.Pile?.Type != PileType.Hand)
        {
            return;
        }

        await CardCmd.AutoPlay(choiceContext, Card, cardPlay.Target);
    }
}
