using System.Collections.Generic;
using System.Threading.Tasks;
using Feiyap.Mechanics;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Feiyap.Powers;

/// <summary>
/// 鬼镰月：本回合居合反击伤害提升 50%。下回合开始时移除，以便敌人回合触发居合反击时仍生效。
/// </summary>
[RegisterPower]
public sealed class FeiyapGuilianMoonPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override PowerAssetProfile AssetProfile =>
        FeiyapPowerAssets.ForSharedIcon(nameof(FeiyapGuilianMoonPower), nameof(FeiyapIaidoRainPower));

    protected override IEnumerable<string> RegisteredKeywordIds => [FeiyapKeywords.IaidoId];

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player.Creature != Owner)
        {
            return;
        }

        await PowerCmd.Remove(this);
    }
}
