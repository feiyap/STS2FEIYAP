using Feiyap.Mechanics;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Patching.Models;

namespace Feiyap.Patches;

/// <summary>
/// 绯夜攻击牌命中时按卡牌主题挂上对应特效与音效。
/// </summary>
public sealed class FeiyapAttackSlashVfxPatch : IPatchMethod
{
    public static string PatchId => "feiyap_attack_slash_vfx";

    public static string Description => "绯夜攻击牌按主题配置命中特效";

    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets() =>
    [
        new(typeof(AttackCommand), nameof(AttackCommand.Execute), [
            typeof(PlayerChoiceContext)
        ])
    ];

    public static void Prefix(AttackCommand __instance)
    {
        if (__instance.ModelSource is not CardModel card)
        {
            return;
        }

        FeiyapAttackHitVfx.TryAttach(__instance, card);
    }
}
