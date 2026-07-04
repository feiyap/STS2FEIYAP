using MegaCrit.Sts2.Core.Entities.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Feiyap.Powers;

/// <summary>
/// XXI-世界（逆位）：所有塔罗牌同时触发正位和逆位效果。
/// </summary>
[RegisterPower]
public sealed class FeiyapTarotWorldDualEffectPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;
}
