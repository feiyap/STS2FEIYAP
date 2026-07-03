using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Danjin.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace Danjin.Potions;

public sealed class TeJiFuSuXiWu : DanjinPotion
{
	private const int FuSuStacks = 4;

	private const decimal HealPercent = 0.10m;

	public override PotionRarity Rarity => (PotionRarity)3;

	public override PotionUsage Usage => (PotionUsage)3;

	public override TargetType TargetType => (TargetType)1;

	public override bool CanBeGeneratedInCombat => false;

	protected override IEnumerable<DynamicVar> CanonicalVars => Array.Empty<DynamicVar>();

	protected override IEnumerable<IHoverTip> AdditionalHoverTips => new _003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.FromPower<FuSuPower>((int?)null));

	public override bool ShouldDie(Creature creature)
	{
		return creature != ((PotionModel)this).Owner.Creature;
	}

	public override async Task AfterPreventingDeath(Creature creature)
	{
		await ((PotionModel)this).OnUseWrapper((PlayerChoiceContext)new ThrowingPlayerChoiceContext(), creature);
	}

	protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
	{
		PotionModel.AssertValidForTargetedPotion(target);
		decimal num = Math.Max((decimal)target.MaxHp * 0.10m, 1m);
		await CreatureCmd.Heal(target, num, true);
		await PowerCmd.Apply<FuSuPower>(choiceContext, target, 4m, ((PotionModel)this).Owner.Creature, (CardModel)null, false);
	}
}
