using MegaCrit.Sts2.Core.Entities.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Feiyap.Powers;

/// <summary>
/// 雨曾为紫：打出攻击牌不再移除活力。
/// </summary>
[RegisterPower]
public sealed class FeiyapPreserveVigorPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override PowerAssetProfile AssetProfile => FeiyapPowerAssets.For(nameof(FeiyapPreserveVigorPower));
}
