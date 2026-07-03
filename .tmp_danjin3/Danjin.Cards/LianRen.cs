using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Danjin.Powers;
using Danjin.Resources;
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

public class LianRen : DanjinCard
{
	public override IEnumerable<CardKeyword> CanonicalKeywords => new _003C_003Ez__ReadOnlySingleElementList<CardKeyword>((CardKeyword)1);

	public override int CanonicalStarCost => 1;

	protected override HashSet<CardTag> CanonicalTags => new HashSet<CardTag> { (CardTag)0 };

	protected override IEnumerable<IHoverTip> AdditionalHoverTips => new _003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.FromPower<ShouShiPower>((int?)null));

	protected override IEnumerable<DynamicVar> CanonicalVars => new _003C_003Ez__ReadOnlySingleElementList<DynamicVar>((DynamicVar)new BlockVar(8m, (ValueProp)8));

	public LianRen()
		: base(1, (CardType)2, (CardRarity)3, (TargetType)1)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await CreatureCmd.GainBlock(((CardModel)this).Owner.Creature, ((CardModel)this).DynamicVars.Block, cardPlay, false);
		CardPile pile = PileTypeExtensions.GetPile((PileType)1, ((CardModel)this).Owner);
		if (pile != null)
		{
			List<CardModel> list = pile.Cards.Where((CardModel c) => (int)c.Type != 1).ToList();
			if (list.Count > 0)
			{
				CardSelectorPrefs val = default(CardSelectorPrefs);
				((CardSelectorPrefs)(ref val))._002Ector(((CardModel)this).SelectionScreenPrompt, 1);
				CardModel val2 = (await CardSelectCmd.FromSimpleGrid(choiceContext, (IReadOnlyList<CardModel>)list, ((CardModel)this).Owner, val)).FirstOrDefault();
				if (val2 != null)
				{
					await CardPileCmd.Add(val2, (PileType)2, (CardPilePosition)1, (AbstractModel)null, false);
				}
			}
		}
		int maxStacks = StanceCmd.GetMaxStacks(((CardModel)this).Owner);
		Creature creature = ((CardModel)this).Owner.Creature;
		int num = ((creature != null) ? creature.GetPowerAmount<ShouShiPower>() : 0);
		if (maxStacks > num)
		{
			await StanceCmd.GainDefenseStance(choiceContext, ((CardModel)this).Owner, maxStacks - num);
		}
	}

	protected override void OnUpgrade()
	{
		((DynamicVar)((CardModel)this).DynamicVars.Block).UpgradeValueBy(3m);
	}
}
