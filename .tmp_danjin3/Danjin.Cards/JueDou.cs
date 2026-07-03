using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Danjin.Extensions;
using Danjin.Powers;
using Danjin.Resources;
using Danjin.Variables;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Danjin.Cards;

public class JueDou : DanjinCard
{
	public override IEnumerable<CardKeyword> CanonicalKeywords => new _003C_003Ez__ReadOnlySingleElementList<CardKeyword>(DanjinCardKeywords.CanXin);

	protected override HashSet<CardTag> CanonicalTags => new HashSet<CardTag> { (CardTag)0 };

	protected override IEnumerable<IHoverTip> AdditionalHoverTips => new _003C_003Ez__ReadOnlyArray<IHoverTip>((IHoverTip[])(object)new IHoverTip[3]
	{
		HoverTipFactory.FromPower<ZhuShiZhiKePower>((int?)null),
		RawHoverTipBuilder.Build("danjin-stance-atk", "攻势1", "打出时需要至少[blue]1[/blue]层[red]攻势[/red]，否则该效果不生效。"),
		RawHoverTipBuilder.Build("danjin-stance-def", "守势1", "打出时需要至少[blue]1[/blue]层[blue]守势[/blue]，否则该效果不生效。")
	});

	protected override IEnumerable<DynamicVar> CanonicalVars => new _003C_003Ez__ReadOnlyArray<DynamicVar>((DynamicVar[])(object)new DynamicVar[2]
	{
		new ZhuShiZhiKeVar(),
		(DynamicVar)new BlockVar(5m, (ValueProp)8)
	});

	public JueDou()
		: base(1, (CardType)2, (CardRarity)3, (TargetType)2)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, "Target");
		Creature creature = ((CardModel)this).Owner.Creature;
		int gongShi = ((creature != null) ? creature.GetPowerAmount<GongShiPower>() : 0);
		Creature creature2 = ((CardModel)this).Owner.Creature;
		int shouShi = ((creature2 != null) ? creature2.GetPowerAmount<ShouShiPower>() : 0);
		await StanceCmd.ReinforceStance(choiceContext, ((CardModel)this).Owner);
		if (gongShi >= 1)
		{
			if (cardPlay.Target.IsAlive)
			{
				await PowerCmd.Apply<ZhuShiZhiKePower>(choiceContext, cardPlay.Target, ((CardModel)this).DynamicVars["ZhuShiZhiKeStacks"].BaseValue, ((CardModel)this).Owner.Creature, (CardModel)(object)this, false);
				await PowerCmd.Apply<JueDouExposePower>(choiceContext, cardPlay.Target, 1m, ((CardModel)this).Owner.Creature, (CardModel)(object)this, false);
			}
		}
		else if (shouShi >= 1)
		{
			await CreatureCmd.GainBlock(((CardModel)this).Owner.Creature, ((CardModel)this).DynamicVars.Block, cardPlay, false);
			if (cardPlay.Target.IsAlive)
			{
				await PowerCmd.Apply<JueDouSuppressPower>(choiceContext, cardPlay.Target, 1m, ((CardModel)this).Owner.Creature, (CardModel)(object)this, false);
			}
		}
	}

	protected override void OnUpgrade()
	{
		((CardModel)this).DynamicVars["ZhuShiZhiKeStacks"].UpgradeValueBy(1m);
		((DynamicVar)((CardModel)this).DynamicVars.Block).UpgradeValueBy(2m);
	}
}
