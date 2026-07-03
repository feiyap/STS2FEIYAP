using System;
using Danjin.Powers;
using Danjin.Resources;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace Danjin.Variables;

public class LiaoYuVar : DynamicVar
{
	public const string DefaultName = "LiaoYuHeal";

	public decimal PercentPerStar { get; }

	public decimal BaseHeal { get; }

	public LiaoYuVar(decimal percentPerStar = 0.03m, decimal baseHeal = 0m)
		: base("LiaoYuHeal", 0m)
	{
		PercentPerStar = percentPerStar;
		BaseHeal = baseHeal;
	}

	public override void UpdateCardPreview(CardModel card, CardPreviewMode previewMode, Creature? target, bool runGlobalHooks)
	{
		Player owner = card.Owner;
		if (((owner != null) ? owner.Creature : null) == null)
		{
			((DynamicVar)this).PreviewValue = 0m;
			return;
		}
		Creature creature = card.Owner.Creature;
		int starCost = Math.Max(0, card.GetStarCostWithModifiers());
		int desired = CalculateDesiredHeal(creature, starCost, PercentPerStar);
		int num = ApplyBloodPoolCap(card.Owner, desired);
		((DynamicVar)this).PreviewValue = num;
	}

	public static int CalculateDesiredHeal(Creature creature, int starCost, decimal percentPerStar = 0.03m)
	{
		if (creature == null || starCost <= 0)
		{
			return 0;
		}
		decimal num = Math.Ceiling((decimal)creature.MaxHp * percentPerStar * (decimal)starCost);
		return Math.Max(0, (int)num);
	}

	public static int ApplyBloodPoolCap(Player? owner, int desired)
	{
		if (owner == null || desired <= 0)
		{
			return 0;
		}
		int val = TonghuaHealPoolCmd.PeekRemaining(owner);
		return Math.Min(desired, val);
	}

	[Obsolete("新机制下没有 BaseHeal 项；直接用 CalculateDesiredHeal。")]
	public static int CalculateBaseHeal(Creature creature, int starCost, decimal percentPerStar = 0.03m, decimal baseHeal = 1m)
	{
		return CalculateDesiredHeal(creature, starCost, percentPerStar);
	}

	[Obsolete("新机制下走血池 cap；外部预览改用 ApplyBloodPoolCap 配合 CalculateDesiredHeal。")]
	public static int CalculateEffectiveHeal(Creature creature, int starCost, decimal percentPerStar = 0.03m, decimal baseHeal = 1m)
	{
		return CalculateDesiredHeal(creature, starCost, percentPerStar);
	}

	[Obsolete("新机制下回血上限由血池 cap 决定，不再走乘数 buff。")]
	public static decimal ApplyHealModifiers(Creature creature, decimal amount)
	{
		if (creature == null || amount <= 0m)
		{
			return amount;
		}
		if (creature.HasPower<ZhouXuePower>())
		{
			amount *= 1m;
		}
		return amount;
	}

	[Obsolete("改用 CalculateDesiredHeal + ApplyBloodPoolCap。")]
	public static int CalculateHeal(Creature creature, int starCost, decimal percentPerStar = 0.03m, decimal baseHeal = 1m)
	{
		return CalculateDesiredHeal(creature, starCost, percentPerStar);
	}
}
