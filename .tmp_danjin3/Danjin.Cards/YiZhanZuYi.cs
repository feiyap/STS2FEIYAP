using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Danjin.Powers;
using Danjin.Variables;
using Danjin.Vfx;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace Danjin.Cards;

public class YiZhanZuYi : DanjinCard
{
	public override int CanonicalStarCost => 4;

	protected override IEnumerable<IHoverTip> AdditionalHoverTips => new _003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.FromPower<GongShiPower>((int?)null));

	protected override HashSet<CardTag> CanonicalTags => new HashSet<CardTag> { (CardTag)0 };

	protected override IEnumerable<DynamicVar> CanonicalVars => new _003C_003Ez__ReadOnlyArray<DynamicVar>((DynamicVar[])(object)new DynamicVar[2]
	{
		(DynamicVar)new YiZhanZuYiDamageVar(20m),
		new DynamicVar("BonusDmgPerGongShi", 5m)
	});

	protected override bool ShouldGlowGoldInternal
	{
		get
		{
			Creature creature = ((CardModel)this).Owner.Creature;
			return ((creature != null && creature.GetPowerAmount<GongShiPower>() != 0) ? 1 : 0) > (false ? 1 : 0);
		}
	}

	public YiZhanZuYi()
		: base(1, (CardType)1, (CardRarity)3, (TargetType)2)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, "Target");
		Creature creature = ((CardModel)this).Owner.Creature;
		int num = ((creature != null) ? creature.GetPowerAmount<GongShiPower>() : 0);
		decimal num2 = ((DynamicVar)((CardModel)this).DynamicVars.Damage).BaseValue + (decimal)num * ((CardModel)this).DynamicVars["BonusDmgPerGongShi"].BaseValue;
		NDimensionSlashVfx slashVfx = PrepareJianQiSlash(cardPlay.Target);
		await DamageCmd.Attack(num2).FromCard((CardModel)(object)this).Targeting(cardPlay.Target)
			.BeforeDamage(DanjinCard.DoSlashHook(slashVfx))
			.Execute(choiceContext);
	}

	protected override void OnUpgrade()
	{
		((DynamicVar)((CardModel)this).DynamicVars.Damage).UpgradeValueBy(10m);
	}
}
