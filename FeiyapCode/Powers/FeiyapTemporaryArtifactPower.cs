using Feiyap.Cards.Uncommon;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Combat.Powers;
using STS2RitsuLib.Interop.AutoRegistration;

namespace Feiyap.Powers;

/// <summary>
/// 侘寂：本回合获得人工制品，敌人回合结束后移除（默认在玩家回合结束移除会导致无法抵挡敌人攻击）。
/// </summary>
[RegisterPower]
public sealed class FeiyapTemporaryArtifactPower : ModTemporaryAppliedPowerTemplate<FeiyapUncommon21, ArtifactPower>
{
    protected override bool UntilEndOfOtherSideTurn => true;
}
