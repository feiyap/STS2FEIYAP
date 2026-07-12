using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Feiyap.Enchantments;

/// <summary>
/// 重燃：每场战斗第一次打出后回到手牌。
/// </summary>
[RegisterEnchantment]
public sealed class FeiyapRekindleEnchantment : ModEnchantmentTemplate
{
    private bool _usedThisCombat;

    public override bool HasExtraCardText => true;

    public override bool CanEnchantCardType(CardType cardType) => cardType == CardType.Skill;

    public override Task AfterRoomEntered(AbstractRoom room)
    {
        if (room is CombatRoom)
        {
            _usedThisCombat = false;
        }

        return Task.CompletedTask;
    }

    public override (PileType, CardPilePosition) ModifyCardPlayResultPileTypeAndPosition(
        CardModel card,
        bool isAutoPlay,
        ResourceInfo resources,
        PileType pileType,
        CardPilePosition position)
    {
        if (card != Card || _usedThisCombat || pileType != PileType.Discard)
        {
            return (pileType, position);
        }

        _usedThisCombat = true;
        return (PileType.Hand, CardPilePosition.Top);
    }
}
