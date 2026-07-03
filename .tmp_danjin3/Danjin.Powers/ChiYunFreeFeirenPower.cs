using Danjin.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;

namespace Danjin.Powers;

public sealed class ChiYunFreeFeirenPower : DanjinPower
{
	public override PowerType Type => (PowerType)1;

	public override PowerStackType StackType => (PowerStackType)1;

	public override bool TryModifyEnergyCostInCombat(CardModel card, decimal originalCost, out decimal modifiedCost)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		if (((PowerModel)this).Amount > 0 && card != null && card.Keywords.Contains(DanjinCardKeywords.FeiRen))
		{
			modifiedCost = default(decimal);
			return true;
		}
		modifiedCost = originalCost;
		return false;
	}
}
