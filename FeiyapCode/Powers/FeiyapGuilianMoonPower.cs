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
/// 鬼镰月：记录本回合居合触发次数，下回合开始时按次数抽牌后移除。
/// </summary>
[RegisterPower]
public sealed class FeiyapGuilianMoonPower : ModPowerTemplate
{
    private int _triggers;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override PowerAssetProfile AssetProfile =>
        FeiyapPowerAssets.ForSharedIcon(nameof(FeiyapGuilianMoonPower), nameof(FeiyapIaidoRainPower));

    protected override IEnumerable<string> RegisteredKeywordIds => [FeiyapKeywords.IaidoId];

    public void RecordTrigger()
    {
        _triggers++;
        Flash();
    }

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player.Creature != Owner)
        {
            return;
        }

        if (_triggers > 0)
        {
            Flash();
            await CardPileCmd.Draw(choiceContext, _triggers, player);
        }

        await PowerCmd.Remove(this);
    }
}
