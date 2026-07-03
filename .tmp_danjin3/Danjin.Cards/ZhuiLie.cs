using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Danjin.Powers;
using Danjin.Resources;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace Danjin.Cards;

public class ZhuiLie : DanjinCard
{
	public override IEnumerable<CardKeyword> CanonicalKeywords => new _003C_003Ez__ReadOnlySingleElementList<CardKeyword>(DanjinCardKeywords.LiuZhuan);

	public override int CanonicalStarCost => 2;

	protected override HashSet<CardTag> CanonicalTags => new HashSet<CardTag> { (CardTag)0 };

	protected override IEnumerable<IHoverTip> AdditionalHoverTips => new _003C_003Ez__ReadOnlyArray<IHoverTip>((IHoverTip[])(object)new IHoverTip[2]
	{
		HoverTipFactory.FromPower<WeakPower>((int?)null),
		HoverTipFactory.FromPower<ZhuShiZhiKePower>((int?)null)
	});

	protected override IEnumerable<DynamicVar> CanonicalVars => new _003C_003Ez__ReadOnlyArray<DynamicVar>((DynamicVar[])(object)new DynamicVar[3]
	{
		(DynamicVar)new BlockVar(7m, (ValueProp)8),
		(DynamicVar)new PowerVar<WeakPower>(1m),
		(DynamicVar)new PowerVar<ZhuShiZhiKePower>(1m)
	});

	public ZhuiLie()
		: base(0, (CardType)2, (CardRarity)1, (TargetType)2)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, "Target");
		await StanceCmd.SwitchStance(choiceContext, ((CardModel)this).Owner);
		await CreatureCmd.GainBlock(((CardModel)this).Owner.Creature, ((CardModel)this).DynamicVars.Block, cardPlay, false);
		Creature creature = ((CardModel)this).Owner.Creature;
		if (creature != null && cardPlay.Target.IsAlive)
		{
			int powerAmount = creature.GetPowerAmount<GongShiPower>();
			if (creature.GetPowerAmount<ShouShiPower>() > 0)
			{
				await PowerCmd.Apply<WeakPower>(choiceContext, cardPlay.Target, ((CardModel)this).DynamicVars["WeakPower"].BaseValue, ((CardModel)this).Owner.Creature, (CardModel)(object)this, false);
			}
			else if (powerAmount > 0)
			{
				await PowerCmd.Apply<ZhuShiZhiKePower>(choiceContext, cardPlay.Target, ((CardModel)this).DynamicVars["ZhuShiZhiKePower"].BaseValue, ((CardModel)this).Owner.Creature, (CardModel)(object)this, false);
			}
		}
	}

	protected override void OnUpgrade()
	{
		((CardModel)this).DynamicVars["WeakPower"].UpgradeValueBy(1m);
		((CardModel)this).DynamicVars["ZhuShiZhiKePower"].UpgradeValueBy(1m);
	}
}
