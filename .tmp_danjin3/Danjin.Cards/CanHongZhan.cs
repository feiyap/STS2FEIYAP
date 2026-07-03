using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Danjin.Extensions;
using Danjin.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Danjin.Cards;

public class CanHongZhan : DanjinCard
{
	protected override IEnumerable<IHoverTip> AdditionalHoverTips => new _003C_003Ez__ReadOnlyArray<IHoverTip>((IHoverTip[])(object)new IHoverTip[2]
	{
		HoverTipFactory.FromPower<ChuXuePower>((int?)null),
		RawHoverTipBuilder.Build("danjin-stance-atk", "攻势1", "打出时需要至少[blue]1[/blue]层[red]攻势[/red]，否则该效果不生效。")
	});

	protected override HashSet<CardTag> CanonicalTags => new HashSet<CardTag> { (CardTag)0 };

	protected override IEnumerable<DynamicVar> CanonicalVars => new _003C_003Ez__ReadOnlyArray<DynamicVar>((DynamicVar[])(object)new DynamicVar[3]
	{
		(DynamicVar)new DamageVar(7m, (ValueProp)8),
		(DynamicVar)new CardsVar(1),
		(DynamicVar)new PowerVar<ChuXuePower>(2m)
	});

	protected override bool ShouldGlowGoldInternal
	{
		get
		{
			Player owner = ((CardModel)this).Owner;
			Creature val = ((owner != null) ? owner.Creature : null);
			if (val == null || ((CardModel)this).CombatState == null)
			{
				return false;
			}
			return val.GetPowerAmount<GongShiPower>() >= 1;
		}
	}

	public CanHongZhan()
		: base(1, (CardType)1, (CardRarity)2, (TargetType)2)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
		Creature creature = ((CardModel)this).Owner.Creature;
		int gongShi = ((creature != null) ? creature.GetPowerAmount<GongShiPower>() : 0);
		await PlayJianQiAttackOnce(choiceContext, cardPlay.Target);
		await CardPileCmd.Draw(choiceContext, ((DynamicVar)((CardModel)this).DynamicVars.Cards).BaseValue, ((CardModel)this).Owner, false);
		if (gongShi >= 1 && cardPlay.Target.IsAlive)
		{
			await PowerCmd.Apply<ChuXuePower>(choiceContext, cardPlay.Target, ((CardModel)this).DynamicVars["ChuXuePower"].BaseValue, ((CardModel)this).Owner.Creature, (CardModel)(object)this, false);
		}
	}

	protected override void OnUpgrade()
	{
		((DynamicVar)((CardModel)this).DynamicVars.Damage).UpgradeValueBy(1m);
		((DynamicVar)((CardModel)this).DynamicVars.Cards).UpgradeValueBy(1m);
	}
}
