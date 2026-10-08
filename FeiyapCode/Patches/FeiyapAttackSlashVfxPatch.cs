using Feiyap.Mechanics;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Patching.Models;

namespace Feiyap.Patches;

/// <summary>
/// 绯夜攻击牌命中时自动挂上原版斩击特效与音效（排除剑鞘打击、分铜锁、枯山水）。
/// </summary>
public sealed class FeiyapAttackSlashVfxPatch : IPatchMethod
{
    public static string PatchId => "feiyap_attack_slash_vfx";

    public static string Description => "绯夜攻击牌命中斩击特效";

    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets() =>
    [
        new(typeof(AttackCommand), nameof(AttackCommand.Execute), [
            typeof(PlayerChoiceContext)
        ])
    ];

    public static void Prefix(AttackCommand __instance)
    {
        if (__instance.ModelSource is not CardModel card
            || !FeiyapSlashCmd.ShouldAttachCardHitSlash(card))
        {
            return;
        }

        FeiyapSlashCmd.AttachCardHitSlash(__instance);
    }
}
