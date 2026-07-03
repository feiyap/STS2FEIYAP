using System;
using Danjin.Powers;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace Danjin.Variables;

public class FeiRenVar : DynamicVar
{
	public const string DefaultName = "FeiRenCost";

	public decimal PerHitPercent { get; }

	public FeiRenVar(decimal percent = 0.03m)
		: base("FeiRenCost", 0m)
	{
		PerHitPercent = percent;
	}

	public override void UpdateCardPreview(CardModel card, CardPreviewMode previewMode, Creature? target, bool runGlobalHooks)
	{
		Player owner = card.Owner;
		if (((owner != null) ? owner.Creature : null) == null)
		{
			((DynamicVar)this).PreviewValue = 0m;
			return;
		}
		decimal previewValue = CalculateEffectiveTotalHpLoss(card.Owner.Creature, hitCount: GetHitCountFromCard(card, target), percent: PerHitPercent);
		((DynamicVar)this).PreviewValue = previewValue;
	}

	private static int GetHitCountFromCard(CardModel card, Creature? target)
	{
		if (card is IFeirenHitCountProvider feirenHitCountProvider)
		{
			return Math.Max(1, feirenHitCountProvider.GetFeirenHitCountForPreview(target));
		}
		DynamicVar val = default(DynamicVar);
		if (card.DynamicVars.TryGetValue("HitCount", ref val))
		{
			return Math.Max(1, (int)val.BaseValue);
		}
		return 1;
	}

	public static int CalculateHpLoss(Creature creature, decimal percent)
	{
		int val = (int)Math.Max(1m, Math.Floor((decimal)creature.MaxHp * percent));
		val = Math.Min(val, creature.CurrentHp - 1);
		return Math.Max(0, val);
	}

	public static int CalculateEffectiveHpLoss(Creature creature, decimal percent)
	{
		if (creature.HasPower<FuSuPower>())
		{
			return 0;
		}
		return CalculateHpLoss(creature, percent);
	}

	public static int CalculateIdealPerHitLoss(Creature creature, decimal percent)
	{
		return (int)Math.Max(1m, Math.Floor((decimal)creature.MaxHp * percent));
	}

	public static int CalculateEffectiveTotalHpLoss(Creature creature, decimal percent, int hitCount)
	{
		if (creature.HasPower<FuSuPower>())
		{
			return 0;
		}
		if (hitCount <= 0)
		{
			return 0;
		}
		int val = CalculateIdealPerHitLoss(creature, percent) * hitCount;
		val = Math.Min(val, creature.CurrentHp - 1);
		return Math.Max(0, val);
	}
}
