using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Feiyap.Powers;

/// <summary>
/// XIV-节制（逆位）：额外获得一个回合。
/// </summary>
[RegisterPower]
public sealed class FeiyapExtraTurnPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile =>
        FeiyapPowerAssets.ForSharedIcon(nameof(FeiyapExtraTurnPower), "FeiyapTarotWorldFreeChoicePower");

    public override bool ShouldTakeExtraTurn(Player player) =>
        player == Owner.Player;

    public override async Task AfterTakingExtraTurn(Player player)
    {
        if (player == Owner.Player)
        {
            await PowerCmd.Decrement(this);
        }
    }
}
