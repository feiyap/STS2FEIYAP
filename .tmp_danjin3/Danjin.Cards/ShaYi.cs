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

namespace Danjin.Cards;

public class ShaYi : DanjinCard
{
	public override IEnumerable<CardKeyword> CanonicalKeywords => new _003C_003Ez__ReadOnlyArray<CardKeyword>((CardKeyword[])(object)new CardKeyword[2]
	{
		(CardKeyword)(int)DanjinCardKeywords.LiuZhuan,
		(CardKeyword)1
	});

	protected override HashSet<CardTag> CanonicalTags => new HashSet<CardTag> { (CardTag)0 };

	protected override IEnumerable<IHoverTip> AdditionalHoverTips => new _003C_003Ez__ReadOnlyArray<IHoverTip>((IHoverTip[])(object)new IHoverTip[2]
	{
		HoverTipFactory.FromPower<ZhuShiZhiKePower>((int?)null),
		HoverTipFactory.FromPower<WeakPower>((int?)null)
	});

	protected override IEnumerable<DynamicVar> CanonicalVars => new _003C_003Ez__ReadOnlyArray<DynamicVar>((DynamicVar[])(object)new DynamicVar[2]
	{
		(DynamicVar)new PowerVar<ZhuShiZhiKePower>(2m),
		(DynamicVar)new PowerVar<WeakPower>(2m)
	});

	public ShaYi()
		: base(0, (CardType)2, (CardRarity)2, (TargetType)2)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, "Target");
		Creature creature = ((CardModel)this).Owner.Creature;
		await StanceCmd.SwitchStance(choiceContext, ((CardModel)this).Owner);
		if (cardPlay.Target.IsAlive)
		{
			int num = ((creature != null) ? creature.GetPowerAmount<GongShiPower>() : 0);
			int num2 = ((creature != null) ? creature.GetPowerAmount<ShouShiPower>() : 0);
			if (num > 0)
			{
				await PowerCmd.Apply<ZhuShiZhiKePower>(choiceContext, cardPlay.Target, ((CardModel)this).DynamicVars["ZhuShiZhiKePower"].BaseValue, ((CardModel)this).Owner.Creature, (CardModel)(object)this, false);
			}
			else if (num2 > 0)
			{
				await PowerCmd.Apply<WeakPower>(choiceContext, cardPlay.Target, ((CardModel)this).DynamicVars["WeakPower"].BaseValue, ((CardModel)this).Owner.Creature, (CardModel)(object)this, false);
			}
		}
	}

	protected override void OnUpgrade()
	{
		((CardModel)this).DynamicVars["ZhuShiZhiKePower"].UpgradeValueBy(1m);
		((CardModel)this).DynamicVars["WeakPower"].UpgradeValueBy(1m);
	}
}
