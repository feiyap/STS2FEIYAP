using System.Threading.Tasks;
using Feiyap.Mechanics;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Feiyap.Powers;

/// <summary>
/// 活杀自在：每获得 <see cref="IaidoPerVigor"/> 点居合，获得 Amount 点活力。
/// 叠层增加活力，不提高居合门槛。
/// </summary>
[RegisterPower]
public sealed class FeiyapKassaiJizaiPower : ModPowerTemplate
{
    /// <summary>兑换 1 次活力所需的居合。与本地化中的固定数字一致。</summary>
    public const int IaidoPerVigor = 2;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => FeiyapPowerAssets.For(nameof(FeiyapKassaiJizaiPower));

    public static async Task OnIaidoGained(
        PlayerChoiceContext choiceContext,
        Creature owner,
        decimal amountGained)
    {
        var power = owner.GetPower<FeiyapKassaiJizaiPower>();
        if (power == null || amountGained <= 0m || power.Amount <= 0)
        {
            return;
        }

        var player = owner.Player;
        if (player == null)
        {
            return;
        }

        var threshold = (decimal)IaidoPerVigor;
        var tracker = FeiyapCombatTracker.Get(player);
        var total = tracker.KassaiJizaiIaidoRemainder + amountGained;
        var vigorGain = Math.Floor(total / threshold) * power.Amount;
        tracker.KassaiJizaiIaidoRemainder = total % threshold;

        if (vigorGain <= 0m)
        {
            return;
        }

        power.Flash();
        await PowerCmd.Apply<VigorPower>(
            choiceContext,
            owner,
            vigorGain,
            owner,
            null);
    }
}
