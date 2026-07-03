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

public class WuWeiYanZhiBuYu : DanjinCard
{
	public override IEnumerable<CardKeyword> CanonicalKeywords => new _003C_003Ez__ReadOnlySingleElementList<CardKeyword>(DanjinCardKeywords.LiuZhuan);

	protected override HashSet<CardTag> CanonicalTags => new HashSet<CardTag> { (CardTag)0 };

	protected override IEnumerable<IHoverTip> AdditionalHoverTips => new _003C_003Ez__ReadOnlyArray<IHoverTip>((IHoverTip[])(object)new IHoverTip[2]
	{
		HoverTipFactory.FromPower<DexterityPower>((int?)null),
		HoverTipFactory.FromPower<StrengthPower>((int?)null)
	});

	protected override IEnumerable<DynamicVar> CanonicalVars => new _003C_003Ez__ReadOnlyArray<DynamicVar>((DynamicVar[])(object)new DynamicVar[3]
	{
		(DynamicVar)new DamageVar(1m, (ValueProp)8),
		(DynamicVar)new PowerVar<WuWeiYanZhiBuYuPower>(2m),
		(DynamicVar)new PowerVar<WuWeiStrengthPower>(2m)
	});

	public WuWeiYanZhiBuYu()
		: base(0, (CardType)1, (CardRarity)2, (TargetType)2)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, "Target");
		Creature creature = ((CardModel)this).Owner.Creature;
		await StanceCmd.SwitchStance(choiceContext, ((CardModel)this).Owner);
		await PlayJianQiAttackOnce(choiceContext, cardPlay.Target);
		int num = ((creature != null) ? creature.GetPowerAmount<GongShiPower>() : 0);
		if (((creature != null && creature.GetPowerAmount<ShouShiPower>() != 0) ? 1 : 0) > (false ? 1 : 0))
		{
			await PowerCmd.Apply<WuWeiYanZhiBuYuPower>(choiceContext, creature, ((CardModel)this).DynamicVars["WuWeiYanZhiBuYuPower"].BaseValue, creature, (CardModel)(object)this, false);
		}
		else if (num > 0)
		{
			await PowerCmd.Apply<WuWeiStrengthPower>(choiceContext, creature, ((CardModel)this).DynamicVars["WuWeiStrengthPower"].BaseValue, creature, (CardModel)(object)this, false);
		}
	}

	protected override void OnUpgrade()
	{
		((CardModel)this).DynamicVars["WuWeiYanZhiBuYuPower"].UpgradeValueBy(1m);
		((CardModel)this).DynamicVars["WuWeiStrengthPower"].UpgradeValueBy(1m);
	}
}
