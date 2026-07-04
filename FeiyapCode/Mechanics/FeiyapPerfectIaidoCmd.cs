using Feiyap.Cards.Rare;
using Feiyap.Cards.Uncommon;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Feiyap.Mechanics;

/// <summary>
/// 完美居合触发时的副作用。
/// </summary>
public static class FeiyapPerfectIaidoCmd
{
    /// <summary>
    /// 当居合数值与本次攻击伤害相等时触发完美居合。
    /// </summary>
    public static Task Trigger(PlayerChoiceContext choiceContext, Creature creature, decimal blockedDamage)
    {
        if (creature.Player is not { } player || blockedDamage <= 0m)
        {
            return Task.CompletedTask;
        }

        var tracker = FeiyapCombatTracker.Get(player);
        tracker.PerfectIaidoActive = true;

        var hand = player.PlayerCombatState?.Hand;
        if (hand == null)
        {
            return Task.CompletedTask;
        }

        foreach (var card in hand.Cards)
        {
            if (card is FeiyapUncommon18 yeYin)
            {
                yeYin.MarkPerfectIaidoWitnessed();
            }

            if (card is FeiyapRare4 karesansui)
            {
                karesansui.MarkPerfectIaidoWitnessed();
            }
        }

        return Task.CompletedTask;
    }

    public static void OnPlayerTurnStart(Player player)
    {
        FeiyapCombatTracker.Get(player).PerfectIaidoActive = false;
    }

    public static void OnIaidoGained(Player player)
    {
        var allCards = player.PlayerCombatState?.AllCards;
        if (allCards == null)
        {
            return;
        }

        foreach (var card in allCards)
        {
            if (card is FeiyapUncommon19 heavenMay)
            {
                heavenMay.OnIaidoGained();
            }
        }
    }
}
