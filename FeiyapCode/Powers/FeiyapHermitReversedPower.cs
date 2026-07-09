using System.Threading.Tasks;
using Feiyap.Mechanics;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Feiyap.Powers;

/// <summary>
/// IX-隐者（逆位）：下一次消耗残心时返还等量残心。
/// </summary>
[RegisterPower]
public sealed class FeiyapHermitReversedPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override PowerAssetProfile AssetProfile =>
        FeiyapPowerAssets.ForSharedIcon(nameof(FeiyapHermitReversedPower), "FeiyapHermitPower");

    public static async Task TryRefundZanxin(
        PlayerChoiceContext choiceContext,
        Creature owner,
        decimal consumed)
    {
        if (consumed <= 0m)
        {
            return;
        }

        var hermit = owner.GetPower<FeiyapHermitReversedPower>();
        if (hermit == null)
        {
            return;
        }

        hermit.Flash();
        await FeiyapZanxinCmd.Gain(choiceContext, owner, consumed, null);
        await PowerCmd.Remove(hermit);
    }
}
