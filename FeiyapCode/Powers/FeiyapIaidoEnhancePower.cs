using System.Collections.Generic;
using Feiyap.Mechanics;
using MegaCrit.Sts2.Core.Entities.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Feiyap.Powers;

/// <summary>
/// 居合强化：每层使居合反击伤害增加 1 点。
/// </summary>
[RegisterPower]
public sealed class FeiyapIaidoEnhancePower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => FeiyapPowerAssets.For(nameof(FeiyapIaidoEnhancePower));

    protected override IEnumerable<string> RegisteredKeywordIds =>
    [
        FeiyapKeywords.IaidoEnhanceId,
        FeiyapKeywords.IaidoId
    ];
}
