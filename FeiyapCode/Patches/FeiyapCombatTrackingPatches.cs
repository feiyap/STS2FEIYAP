using Feiyap.Mechanics;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Patching.Models;

namespace Feiyap.Patches;

/// <summary>
/// 将战斗追踪从予取遗物解耦，通过全局 Hook 为持有绯夜氏卡牌的玩家更新状态。
/// </summary>
public sealed class FeiyapCombatTrackingAfterCardPlayedPatch : IPatchMethod
{
    public static string PatchId => "feiyap_combat_tracking_after_card_played";

    public static string Description => "全局追踪出牌";

    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets() =>
    [
        new(typeof(Hook), nameof(Hook.AfterCardPlayed), [
            typeof(ICombatState),
            typeof(MegaCrit.Sts2.Core.GameActions.Multiplayer.PlayerChoiceContext),
            typeof(CardPlay)
        ])
    ];

    public static void Postfix(CardPlay cardPlay) =>
        FeiyapCombatTrackingService.OnAfterCardPlayed(cardPlay);
}

public sealed class FeiyapCombatTrackingAfterDamageGivenPatch : IPatchMethod
{
    public static string PatchId => "feiyap_combat_tracking_after_damage_given";

    public static string Description => "全局追踪造成伤害";

    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets() =>
    [
        new(typeof(Hook), nameof(Hook.AfterDamageGiven), [
            typeof(MegaCrit.Sts2.Core.GameActions.Multiplayer.PlayerChoiceContext),
            typeof(ICombatState),
            typeof(Creature),
            typeof(DamageResult),
            typeof(ValueProp),
            typeof(Creature),
            typeof(MegaCrit.Sts2.Core.Models.CardModel)
        ])
    ];

    public static void Postfix(Creature? dealer, DamageResult results) =>
        FeiyapCombatTrackingService.OnAfterDamageGiven(dealer, results);
}

public sealed class FeiyapCombatTrackingBeforeSideTurnStartPatch : IPatchMethod
{
    public static string PatchId => "feiyap_combat_tracking_before_side_turn_start";

    public static string Description => "全局追踪回合开始";

    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets() =>
    [
        new(typeof(Hook), nameof(Hook.BeforeSideTurnStart), [
            typeof(ICombatState),
            typeof(CombatSide),
            typeof(IReadOnlyList<Creature>)
        ])
    ];

    public static void Postfix(CombatSide side, IReadOnlyList<Creature> participants) =>
        FeiyapCombatTrackingService.OnBeforeSideTurnStart(side, participants);
}

public sealed class FeiyapCombatTrackingAfterCombatEndPatch : IPatchMethod
{
    public static string PatchId => "feiyap_combat_tracking_after_combat_end";

    public static string Description => "全局清理战斗追踪";

    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets() =>
    [
        new(typeof(Hook), nameof(Hook.AfterCombatEnd), [
            typeof(IRunState),
            typeof(ICombatState),
            typeof(CombatRoom)
        ])
    ];

    public static void Postfix(ICombatState? combatState) =>
        FeiyapCombatTrackingService.OnAfterCombatEnd(combatState);
}
