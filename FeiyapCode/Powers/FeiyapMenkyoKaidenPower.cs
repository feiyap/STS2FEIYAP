using MegaCrit.Sts2.Core.Entities.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Feiyap.Powers;

/// <summary>
/// 免许皆传：居合反击伤害提升（Amount 为百分比加算，50=×1.5，100=×2.0）。
/// </summary>
[RegisterPower]
public sealed class FeiyapMenkyoKaidenPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override PowerAssetProfile AssetProfile =>
        FeiyapPowerAssets.ForSharedIcon(nameof(FeiyapMenkyoKaidenPower), "FeiyapSwordSaintHeartPower");
}
