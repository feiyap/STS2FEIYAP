using Feiyap.Cards.Rare;
using MegaCrit.Sts2.Core.Entities.Players;

namespace Feiyap.Mechanics;

/// <summary>
/// 无想斩：居合抵消伤害时的战斗内伤害成长。
/// </summary>
public static class FeiyapMusouCmd
{
    public static void OnIaidoBlocked(Player player, int blockedAmount)
    {
        if (blockedAmount <= 0)
        {
            return;
        }

        var allCards = player.PlayerCombatState?.AllCards;
        if (allCards == null)
        {
            return;
        }

        foreach (var card in allCards)
        {
            if (card is FeiyapRare2 musou)
            {
                musou.AddBlockedDamage(blockedAmount);
            }
        }
    }
}
