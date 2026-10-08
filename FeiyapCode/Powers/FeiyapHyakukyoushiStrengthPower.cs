using Feiyap.Cards.Uncommon;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Combat.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Feiyap.Powers;

/// <summary>
/// 百巧手（力量）：本回合获得力量。
/// </summary>
[RegisterPower]
public sealed class FeiyapHyakukyoushiStrengthPower : ModTemporaryAppliedPowerTemplate<FeiyapUncommon34, StrengthPower>
{
    public override PowerAssetProfile AssetProfile => FeiyapPowerAssets.For(nameof(FeiyapHyakukyoushiStrengthPower));

    public override LocString Title => new LocString("powers", Id.Entry + ".title");
}
