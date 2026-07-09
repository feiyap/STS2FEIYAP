using System.Collections.Concurrent;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;

namespace Feiyap.Mechanics;

/// <summary>
/// 绯夜氏战斗中的交替出牌与居合追踪状态。
/// </summary>
public sealed class FeiyapCombatTracker
{
    private static readonly ConcurrentDictionary<ulong, FeiyapCombatTracker> Trackers = new();

    public CardType? LastPlayedType { get; set; }

  /// <summary>交替使用攻击/技能后的加成是否激活。</summary>
    public bool AlternateBonusActive { get; set; }

  /// <summary>本回合是否已触发完美居合。</summary>
    public bool PerfectIaidoActive { get; set; }

    /// <summary>本回合是否已打出过星座牌。</summary>
    public bool ConstellationPlayedThisTurn { get; set; }

    /// <summary>下回合开始时保留居合而不清空。</summary>
    public bool RetainIaidoNextTurn { get; set; }

    /// <summary>本回合造成的伤害总量。</summary>
    public int TurnDamageDealt { get; set; }

    /// <summary>本场战斗每次获得居合时的额外加算（如幾星霜）。</summary>
    public decimal IaidoGainCombatBonus { get; set; }

    public void OnTurnStart(Player player) =>
        TurnDamageDealt = 0;

    public void RecordDamageDealt(int amount)
    {
        if (amount > 0)
        {
            TurnDamageDealt += amount;
        }
    }

    public static FeiyapCombatTracker Get(Player player)
    {
        return Trackers.GetOrAdd(player.NetId, _ => new FeiyapCombatTracker());
    }

    public static void ClearCombatState(Player player)
    {
        var tracker = Get(player);
        tracker.LastPlayedType = null;
        tracker.AlternateBonusActive = false;
        tracker.PerfectIaidoActive = false;
        tracker.ConstellationPlayedThisTurn = false;
        tracker.RetainIaidoNextTurn = false;
        tracker.TurnDamageDealt = 0;
        tracker.IaidoGainCombatBonus = 0m;
    }

    public void RecordCardPlayed(CardType type)
    {
        if (type is not (CardType.Attack or CardType.Skill))
        {
            return;
        }

        if (LastPlayedType is CardType.Attack or CardType.Skill && LastPlayedType != type)
        {
            AlternateBonusActive = true;
        }
        else
        {
            AlternateBonusActive = false;
        }

        LastPlayedType = type;
    }
}
