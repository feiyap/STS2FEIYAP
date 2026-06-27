using System.Collections.Generic;
using Feiyap.Mechanics;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using STS2RitsuLib.Patching.Models;

namespace Feiyap.Patches;

/// <summary>
/// 敌人攻击意图预览期间跳过居合减伤，避免意图数字被错误降低。
/// </summary>
public sealed class FeiyapIaidoIntentPreviewPatch : IPatchMethod
{
    public static string PatchId => "feiyap_iaido_intent_preview_scope";

    public static string Description => "居合意图预览作用域";

    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets() =>
    [
        new(typeof(AttackIntent), nameof(AttackIntent.GetSingleDamage), [
            typeof(IEnumerable<Creature>),
            typeof(Creature)
        ])
    ];

    public static void Prefix() => FeiyapIaidoIntentPreviewScope.Enter();

    public static void Postfix() => FeiyapIaidoIntentPreviewScope.Exit();
}
