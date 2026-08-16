using System.Collections.Generic;
using Feiyap.Mechanics;
using MegaCrit.Sts2.Core.Entities.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Feiyap.Powers;

/// <summary>
/// 剑圣心：居合反击对所有敌人造成伤害。
/// </summary>
[RegisterPower]
public sealed class FeiyapSwordSaintHeartPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override PowerAssetProfile AssetProfile => FeiyapPowerAssets.For(nameof(FeiyapSwordSaintHeartPower));

    protected override IEnumerable<string> RegisteredKeywordIds => [FeiyapKeywords.IaidoId];
}
