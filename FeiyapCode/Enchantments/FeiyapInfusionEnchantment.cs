using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Feiyap.Enchantments;

/// <summary>
/// 注能：战斗首回合开始时自动打出（能力牌）。
/// </summary>
[RegisterEnchantment]
public sealed class FeiyapInfusionEnchantment : ModEnchantmentTemplate
{
    public override bool ShouldStartAtBottomOfDrawPile => true;

    public override bool ShowAmount => false;

    public override bool CanEnchantCardType(CardType cardType) => cardType == CardType.Power;

    public override async Task AfterAutoPrePlayPhaseEntered(PlayerChoiceContext choiceContext, Player player)
    {
        if (player == Card.Owner && player.PlayerCombatState.TurnNumber <= 1)
        {
            await CardCmd.AutoPlay(choiceContext, Card, null);
        }
    }
}
