using System.Collections.Generic;
using System.Threading.Tasks;
using Feiyap.Mechanics;
using Feiyap.Patches;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Feiyap.Powers;

/// <summary>
/// 无限居合：本回合居合视为 ∞，不参与数值累计，下回合开始时移除。
/// </summary>
[RegisterPower]
public sealed class FeiyapInfiniteIaidoPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override PowerAssetProfile AssetProfile =>
        FeiyapPowerAssets.ForSharedIcon(nameof(FeiyapInfiniteIaidoPower), nameof(FeiyapIaidoSurgePower));

    protected override IEnumerable<string> RegisteredKeywordIds => [FeiyapKeywords.IaidoId];

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player.Creature != Owner)
        {
            return;
        }

        await PowerCmd.Remove(this);
        IaidoHealthBarOverlay.RefreshForCreature(Owner);
    }
}
