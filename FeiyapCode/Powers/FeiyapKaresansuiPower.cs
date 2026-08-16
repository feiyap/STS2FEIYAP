using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Feiyap.Powers;

/// <summary>
/// 枯山水：无法获得格挡，且无法获得特定防御类能力。
/// </summary>
[RegisterPower]
public sealed class FeiyapKaresansuiPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Debuff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override PowerAssetProfile AssetProfile =>
        FeiyapPowerAssets.ForSharedIcon(nameof(FeiyapKaresansuiPower), "FeiyapSwordSaintHeartPower");

    public override Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        Owner.PowerApplied += OnPowerApplied;
        return StripForbiddenPowers();
    }

    public override Task AfterRemoved(Creature oldOwner)
    {
        oldOwner.PowerApplied -= OnPowerApplied;
        return Task.CompletedTask;
    }

    public override decimal ModifyBlockMultiplicative(
        Creature target,
        decimal block,
        ValueProp props,
        CardModel? cardSource,
        CardPlay? cardPlay) =>
        target == Owner ? 0m : 1m;

    private void OnPowerApplied(PowerModel power)
    {
        if (IsForbiddenPower(power))
        {
            _ = PowerCmd.Remove(power);
        }
    }

    internal static async Task StripForbiddenPowers(Creature creature)
    {
        foreach (var power in creature.Powers.ToList())
        {
            if (IsForbiddenPower(power))
            {
                await PowerCmd.Remove(power);
            }
        }
    }

    private Task StripForbiddenPowers() => StripForbiddenPowers(Owner);

    private static bool IsForbiddenPower(PowerModel power) =>
        power is SlipperyPower
            or BufferPower
            or HardToKillPower
            or HardenedShellPower;
}
