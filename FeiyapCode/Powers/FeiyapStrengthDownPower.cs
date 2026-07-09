using Feiyap.Cards.Uncommon;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Combat.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Feiyap.Powers;

/// <summary>
/// XI-力量（逆位）：目标本回合减少力量。
/// </summary>
[RegisterPower]
public sealed class FeiyapStrengthDownPower : ModTemporaryAppliedPowerTemplate<FeiyapUncommon11, StrengthPower>
{
    public override PowerAssetProfile AssetProfile => FeiyapPowerAssets.For(nameof(FeiyapStrengthDownPower));
    protected override bool IsPositive => false;
}
