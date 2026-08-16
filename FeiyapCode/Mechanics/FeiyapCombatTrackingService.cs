using Feiyap.Cards;
using Feiyap.Cards.Tarot;
using System.Linq;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;

namespace Feiyap.Mechanics;

/// <summary>
/// 绯夜氏战斗追踪：与遗物解耦，确保其他角色获得绯夜氏卡牌时机制仍正常。
/// </summary>
public static class FeiyapCombatTrackingService
{
    public static void OnAfterCardPlayed(CardPlay cardPlay)
    {
        var owner = cardPlay.Card.Owner;
        if (owner == null || !PlayerHasFeiyapCards(owner))
        {
            return;
        }

        FeiyapQuestProgress.RecordCardPlayed(owner, cardPlay.Card.Type);

        if (FeiyapCardTags.HasConstellation(cardPlay.Card))
        {
            FeiyapCombatTracker.Get(owner).ConstellationPlayedThisTurn = true;
        }

        if (cardPlay.Card.Type == CardType.Attack)
        {
            FeiyapCombatTracker.Get(owner).RecordAttackPlayed();
        }

        // 出牌后正/逆位可触发状态可能变化，立即刷新手牌塔罗卡图。
        FeiyapTarotCardBase.RefreshHandPortraits(owner);
    }

    public static void OnAfterDamageGiven(Creature? dealer, DamageResult result)
    {
        if (dealer?.Player is not { } player
            || !PlayerHasFeiyapCards(player))
        {
            return;
        }

        // 计入格挡吸收与溢出，与攻击实际结算量一致。
        var dealt = result.TotalDamage + result.OverkillDamage;
        if (dealt <= 0)
        {
            return;
        }

        FeiyapCombatTracker.Get(player).RecordDamageDealt(dealt);
    }

    public static void OnBeforeSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants)
    {
        foreach (var participant in participants)
        {
            if (participant.Player is not { } player || participant.Side != side)
            {
                continue;
            }

            if (!PlayerHasFeiyapCards(player))
            {
                continue;
            }

            FeiyapCombatTracker.Get(player).ConstellationPlayedThisTurn = false;
            FeiyapPerfectIaidoCmd.OnPlayerTurnStart(player);
        }
    }

    public static void OnAfterCombatEnd(ICombatState? combatState)
    {
        if (combatState == null)
        {
            return;
        }

        foreach (var player in combatState.Players)
        {
            FeiyapCombatTracker.ClearCombatState(player);
        }
    }

    private static bool PlayerHasFeiyapCards(Player player)
    {
        if (player.Deck.Cards.Any(card => card is FeiyapCardTemplate))
        {
            return true;
        }

        var combatState = player.PlayerCombatState;
        if (combatState == null)
        {
            return false;
        }

        return combatState.AllCards.Any(card => card is FeiyapCardTemplate);
    }
}
