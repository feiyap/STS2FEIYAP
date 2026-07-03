using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;

namespace Danjin.Powers;

public sealed class ManHuangZhiDiDeFeiSePower : DanjinPower
{
	public int TotalEnergyBonus { get; set; }

	public override PowerType Type => (PowerType)1;

	public override PowerStackType StackType => (PowerStackType)1;

	protected override IEnumerable<IHoverTip> AdditionalHoverTips => new _003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.ForEnergy((PowerModel)(object)this));

	public override decimal ModifyMaxEnergy(Player player, decimal amount)
	{
		Creature owner = ((PowerModel)this).Owner;
		if (player != ((owner != null) ? owner.Player : null))
		{
			return amount;
		}
		return amount + (decimal)TotalEnergyBonus;
	}
}
