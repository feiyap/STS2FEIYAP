using Danjin.Resources;
using Godot;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;

namespace Danjin.Powers;

public sealed class ShouShiPower : DanjinPower
{
	public override PowerType Type => (PowerType)1;

	public override PowerStackType StackType => (PowerStackType)1;

	public override Color AmountLabelColor
	{
		get
		{
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			Creature owner = ((PowerModel)this).Owner;
			if (((owner != null) ? owner.Player : null) != null && ((PowerModel)this).Amount >= StanceCmd.GetMaxStacks(((PowerModel)this).Owner.Player))
			{
				return StsColors.gold;
			}
			return StsColors.blue;
		}
	}
}
