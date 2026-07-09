using MegaCrit.Sts2.Core.Entities.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Feiyap.Powers;

/// <summary>
/// IX-隐者（正位）：下一次消耗活力时不扣除（等效于返还等量活力）。
/// </summary>
[RegisterPower]
public sealed class FeiyapHermitUprightPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override PowerAssetProfile AssetProfile =>
        FeiyapPowerAssets.ForSharedIcon(nameof(FeiyapHermitUprightPower), "FeiyapHermitPower");
}
