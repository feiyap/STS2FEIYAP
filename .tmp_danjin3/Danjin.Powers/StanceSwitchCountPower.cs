using MegaCrit.Sts2.Core.Entities.Powers;

namespace Danjin.Powers;

public sealed class StanceSwitchCountPower : DanjinPower
{
	public override PowerType Type => (PowerType)1;

	public override PowerStackType StackType => (PowerStackType)1;

	protected override bool IsVisibleInternal => false;
}
