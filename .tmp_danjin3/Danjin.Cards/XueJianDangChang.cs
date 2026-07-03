using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Danjin.Powers;
using Danjin.Resources;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Danjin.Cards;

public class XueJianDangChang : DanjinCard
{
	protected override IEnumerable<IHoverTip> AdditionalHoverTips => new _003C_003Ez__ReadOnlyArray<IHoverTip>((IHoverTip[])(object)new IHoverTip[2]
	{
		HoverTipFactory.FromPower<GongShiPower>((int?)null),
		HoverTipFactory.Static((StaticHoverTip)6, Array.Empty<DynamicVar>())
	});

	protected override HashSet<CardTag> CanonicalTags => new HashSet<CardTag> { (CardTag)0 };

	protected override IEnumerable<DynamicVar> CanonicalVars => new _003C_003Ez__ReadOnlySingleElementList<DynamicVar>((DynamicVar)new DamageVar(30m, (ValueProp)8));

	public XueJianDangChang()
		: base(3, (CardType)1, (CardRarity)3, (TargetType)2)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, "Target");
		bool shouldTriggerFatal = cardPlay.Target.Powers.All((PowerModel p) => p.ShouldOwnerDeathTriggerFatal());
		AttackCommand val = await PlayJianQiAttackOnce(choiceContext, cardPlay.Target);
		if (shouldTriggerFatal && val != null && val.Results.SelectMany((List<DamageResult> r) => r).Any((DamageResult r) => r.WasTargetKilled))
		{
			await TonghuaCmd.GainTonghua(3m, ((CardModel)this).Owner);
			int maxStacks = StanceCmd.GetMaxStacks(((CardModel)this).Owner);
			Creature creature = ((CardModel)this).Owner.Creature;
			int num = ((creature != null) ? creature.GetPowerAmount<GongShiPower>() : 0);
			if (maxStacks > num)
			{
				await StanceCmd.GainAttackStance(choiceContext, ((CardModel)this).Owner, maxStacks - num);
			}
		}
	}

	protected override void OnUpgrade()
	{
		((DynamicVar)((CardModel)this).DynamicVars.Damage).UpgradeValueBy(8m);
	}
}
