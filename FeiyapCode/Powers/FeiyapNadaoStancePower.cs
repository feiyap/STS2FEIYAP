using System.Collections.Generic;
using System.Threading.Tasks;
using Feiyap.Mechanics;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Scaffolding.Content;

namespace Feiyap.Powers;

/// <summary>
/// 纳刀势：回合开始时获得残心。
/// </summary>
[RegisterPower]
public sealed class FeiyapNadaoStancePower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override PowerAssetProfile AssetProfile =>
        FeiyapPowerAssets.ForSharedIcon(nameof(FeiyapNadaoStancePower), "FeiyapSwordSaintHeartPower");

    protected override IEnumerable<string> RegisteredKeywordIds => [FeiyapKeywords.ZanxinId];

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player.Creature != Owner || Amount <= 0)
        {
            return;
        }

        Flash();
        await FeiyapZanxinCmd.Gain(choiceContext, Owner, Amount, null);
    }
}
