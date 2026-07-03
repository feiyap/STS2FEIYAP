using Danjin.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Danjin.Powers;

public sealed class JianQiRuHongPower : DanjinPower
{
	public override PowerType Type => (PowerType)1;

	public override PowerStackType StackType => (PowerStackType)1;

	public override decimal ModifyDamageAdditive(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		if (dealer == ((PowerModel)this).Owner && ValuePropExtensions.IsPoweredAttack(props) && cardSource != null && cardSource.Keywords.Contains(DanjinCardKeywords.FeiRen))
		{
			return ((PowerModel)this).Amount;
		}
		return 0m;
	}
}
