using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Danjin.Extensions;
using Danjin.Powers;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Danjin.Cards;

public class SuiJiYingBian : DanjinCard
{
	protected override HashSet<CardTag> CanonicalTags => new HashSet<CardTag> { (CardTag)0 };

	protected override IEnumerable<DynamicVar> CanonicalVars => new _003C_003Ez__ReadOnlyArray<DynamicVar>((DynamicVar[])(object)new DynamicVar[2]
	{
		new DynamicVar("FengMang", 3m),
		new DynamicVar("Block", 7m)
	});

	protected override IEnumerable<IHoverTip> AdditionalHoverTips => new _003C_003Ez__ReadOnlyArray<IHoverTip>((IHoverTip[])(object)new IHoverTip[3]
	{
		HoverTipFactory.FromPower<FengMangPower>((int?)null),
		RawHoverTipBuilder.Build("danjin-stance-atk", "攻势1", "打出时需要至少[blue]1[/blue]层[red]攻势[/red]，否则该效果不生效。"),
		RawHoverTipBuilder.Build("danjin-stance-def", "守势1", "打出时需要至少[blue]1[/blue]层[blue]守势[/blue]，否则该效果不生效。")
	});

	public SuiJiYingBian()
		: base(1, (CardType)2, (CardRarity)2, (TargetType)1)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		Creature creature = ((CardModel)this).Owner.Creature;
		int gongShi = ((creature != null) ? creature.GetPowerAmount<GongShiPower>() : 0);
		int shouShi = ((creature != null) ? creature.GetPowerAmount<ShouShiPower>() : 0);
		CardPile pile = PileTypeExtensions.GetPile((PileType)3, ((CardModel)this).Owner);
		if (pile != null && pile.Cards.Count > 0)
		{
			CardSelectorPrefs val = default(CardSelectorPrefs);
			((CardSelectorPrefs)(ref val))._002Ector(((CardModel)this).SelectionScreenPrompt, 1);
			CardModel val2 = (await CardSelectCmd.FromSimpleGrid(choiceContext, pile.Cards, ((CardModel)this).Owner, val)).FirstOrDefault();
			if (val2 != null)
			{
				await CardPileCmd.Add(val2, (PileType)1, (CardPilePosition)2, (AbstractModel)null, false);
			}
		}
		if (gongShi >= 1 && creature != null)
		{
			await PowerCmd.Apply<FengMangPower>(choiceContext, creature, ((CardModel)this).DynamicVars["FengMang"].BaseValue, creature, (CardModel)(object)this, false);
		}
		if (shouShi >= 1 && creature != null)
		{
			await CreatureCmd.GainBlock(creature, ((CardModel)this).DynamicVars["Block"].BaseValue, (ValueProp)8, cardPlay, false);
		}
	}

	protected override void OnUpgrade()
	{
		((CardModel)this).DynamicVars["FengMang"].UpgradeValueBy(1m);
		((CardModel)this).DynamicVars["Block"].UpgradeValueBy(2m);
	}
}
