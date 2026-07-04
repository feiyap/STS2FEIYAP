using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Feiyap.Powers;

/// <summary>
/// 活杀自在：每获得 1 点居合，获得等量活力（Amount 为倍率）。
/// </summary>
[RegisterPower]
public sealed class FeiyapKassaiJizaiPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public static async Task OnIaidoGained(
        PlayerChoiceContext choiceContext,
        Creature owner,
        decimal amountGained)
    {
        var power = owner.GetPower<FeiyapKassaiJizaiPower>();
        if (power == null || amountGained <= 0m || power.Amount <= 0m)
        {
            return;
        }

        power.Flash();
        await PowerCmd.Apply<VigorPower>(
            choiceContext,
            owner,
            amountGained * power.Amount,
            owner,
            null);
    }
}
