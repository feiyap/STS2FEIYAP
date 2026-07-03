using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;

namespace Danjin.Powers;

public sealed class ZhuShiInvalidatedPower : DanjinPower
{
	public override PowerType Type => (PowerType)1;

	public override PowerStackType StackType => (PowerStackType)2;

	public override bool TryModifyPowerAmountReceived(PowerModel canonicalPower, Creature target, decimal amount, Creature? applier, out decimal modifiedAmount)
	{
		if (canonicalPower is ZhuShiZhiKePower && applier == ((PowerModel)this).Owner && amount > 0m)
		{
			modifiedAmount = default(decimal);
			return true;
		}
		modifiedAmount = amount;
		return false;
	}
}
