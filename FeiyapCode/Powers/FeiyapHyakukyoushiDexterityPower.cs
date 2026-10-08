using Feiyap.Cards.Uncommon;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Combat.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Feiyap.Powers;

/// <summary>
/// 百巧手（敏捷）：本回合获得敏捷。
/// </summary>
[RegisterPower]
public sealed class FeiyapHyakukyoushiDexterityPower : ModTemporaryAppliedPowerTemplate<FeiyapUncommon34, DexterityPower>
{
    public override PowerAssetProfile AssetProfile => FeiyapPowerAssets.For(nameof(FeiyapHyakukyoushiDexterityPower));

    public override LocString Title => new LocString("powers", Id.Entry + ".title");
}
