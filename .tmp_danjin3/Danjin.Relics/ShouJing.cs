using Danjin.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Danjin.Relics;

public sealed class ShouJing : DanjinRelic
{
	private const int ExtraDamagePerHit = 2;

	public override RelicRarity Rarity => (RelicRarity)3;

	public override decimal ModifyDamageAdditive(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		if (!ValuePropExtensions.IsPoweredAttack(props))
		{
			return 0m;
		}
		if (cardSource == null)
		{
			return 0m;
		}
		if (dealer != null)
		{
			Player owner = ((RelicModel)this).Owner;
			if (dealer == ((owner != null) ? owner.Creature : null))
			{
				if (cardSource.Owner != ((RelicModel)this).Owner)
				{
					return 0m;
				}
				if (!cardSource.Keywords.Contains(DanjinCardKeywords.FeiRen))
				{
					return 0m;
				}
				return 2m;
			}
		}
		return 0m;
	}
}
