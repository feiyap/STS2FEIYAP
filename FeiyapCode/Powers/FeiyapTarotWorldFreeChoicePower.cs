using MegaCrit.Sts2.Core.Entities.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Feiyap.Powers;

/// <summary>
/// XXI-世界（正位）：所有塔罗牌可自由选择触发正位或逆位效果。
/// </summary>
[RegisterPower]
public sealed class FeiyapTarotWorldFreeChoicePower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override PowerAssetProfile AssetProfile => FeiyapPowerAssets.For(nameof(FeiyapTarotWorldFreeChoicePower));
}
