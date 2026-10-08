using System.Linq;
using Feiyap.Cards.Quest;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;

namespace Feiyap.Mechanics;

/// <summary>
/// 转职任务牌进度更新。
/// </summary>
public static class FeiyapQuestProgress
{
    /// <summary>累计居合反击造成的伤害。</summary>
    public static void RecordIaidoDamage(Player player, int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        foreach (var quest in player.Deck.Cards.OfType<KuXiu>())
        {
            if (!quest.IsComplete)
            {
                quest.AddProgress(amount);
            }
        }
    }

    /// <summary>累计一次成功的攻击/技能交替出牌。</summary>
    public static void RecordAlternateCast(Player player)
    {
        foreach (var quest in player.Deck.Cards.OfType<FaWei>())
        {
            if (!quest.IsComplete)
            {
                quest.AddProgress(1);
            }
        }
    }

    /// <summary>记录出牌并更新交替出牌追踪与乏味任务进度。</summary>
    public static void RecordCardPlayed(Player player, CardType type)
    {
        var tracker = FeiyapCombatTracker.Get(player);
        var alternated = type is CardType.Attack or CardType.Skill
            && tracker.LastPlayedType is CardType.Attack or CardType.Skill
            && tracker.LastPlayedType != type;

        tracker.RecordCardPlayed(type);

        if (alternated)
        {
            RecordAlternateCast(player);
        }
    }
}
