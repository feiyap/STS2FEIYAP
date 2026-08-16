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

    /// <summary>本回合已打出的攻击牌数量（不含当前正在结算的牌）。</summary>
    public int AttacksPlayedThisTurn { get; set; }

    /// <summary>本回合是否已通过绯神乐解锁神座屠。</summary>
    public bool ShinzatoUnlockedThisTurn { get; set; }

    /// <summary>剩余可同时触发正逆位的塔罗出牌次数（莲生双面）。</summary>
    public int DualTarotPlaysRemaining { get; set; }

    /// <summary>活杀自在：累计未兑换活力的居合余数。</summary>
    public decimal KassaiJizaiIaidoRemainder { get; set; }

    public void OnTurnStart(Player player)
    {
        TurnDamageDealt = 0;
        AttacksPlayedThisTurn = 0;
        ShinzatoUnlockedThisTurn = false;
    }

    public void RecordDamageDealt(int amount)
    {
        if (amount > 0)
        {
            TurnDamageDealt += amount;
        }
    }

    public void RecordAttackPlayed()
    {
        AttacksPlayedThisTurn++;
    }

    public static FeiyapCombatTracker Get(Player player) =>
        Trackers.GetOrAdd(player.NetId, _ => new FeiyapCombatTracker());

    public static void ClearCombatState(Player player)
    {
        var tracker = Get(player);
        tracker.LastPlayedType = null;
        tracker.AlternateBonusActive = false;
        tracker.PerfectIaidoActive = false;
        tracker.ConstellationPlayedThisTurn = false;
        tracker.RetainIaidoNextTurn = false;
        tracker.TurnDamageDealt = 0;
        tracker.AttacksPlayedThisTurn = 0;
        tracker.ShinzatoUnlockedThisTurn = false;
        tracker.DualTarotPlaysRemaining = 0;
        tracker.KassaiJizaiIaidoRemainder = 0m;
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
