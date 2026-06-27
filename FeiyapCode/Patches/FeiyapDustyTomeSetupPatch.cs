using Feiyap.Cards.Ancients;
using Feiyap.Characters;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using STS2RitsuLib.Patching.Models;

namespace Feiyap.Patches;

/// <summary>
/// 绯夜氏获得尘封魔典时固定指向先古卡「雨曾为紫」。
/// </summary>
public sealed class FeiyapDustyTomeSetupPatch : IPatchMethod
{
    public static string PatchId => "feiyap_dusty_tome_yu_ceng_wei_zi";

    public static string Description => "尘封魔典固定给予雨曾为紫";

    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets() =>
    [
        new(typeof(DustyTome), nameof(DustyTome.SetupForPlayer), [typeof(Player)])
    ];

    public static bool Prefix(DustyTome __instance, Player player)
    {
        if (player.Character is not FeiyapCharacter)
        {
            return true;
        }

        __instance.AncientCard = ModelDb.Card<YuCengWeiZi>().Id;
        return false;
    }
}
