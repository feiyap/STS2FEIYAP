using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Danjin.Cards;

public class WanMeiShanBi : DanjinCard
{
	protected override HashSet<CardTag> CanonicalTags => new HashSet<CardTag> { (CardTag)0 };

	protected override IEnumerable<DynamicVar> CanonicalVars => new _003C_003Ez__ReadOnlyArray<DynamicVar>((DynamicVar[])(object)new DynamicVar[2]
	{
		(DynamicVar)new BlockVar(8m, (ValueProp)8),
		(DynamicVar)new CardsVar(1)
	});

	public WanMeiShanBi()
		: base(1, (CardType)2, (CardRarity)2, (TargetType)1)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await CreatureCmd.GainBlock(((CardModel)this).Owner.Creature, ((CardModel)this).DynamicVars.Block, cardPlay, false);
		await CardPileCmd.Draw(choiceContext, ((DynamicVar)((CardModel)this).DynamicVars.Cards).BaseValue, ((CardModel)this).Owner, false);
		CardPile pile = PileTypeExtensions.GetPile((PileType)2, ((CardModel)this).Owner);
		if (pile == null || pile.Cards.Count <= 0)
		{
			return;
		}
		CardSelectorPrefs val = new CardSelectorPrefs(((CardModel)this).SelectionScreenPrompt, 1);
		foreach (CardModel item in await CardSelectCmd.FromHand(choiceContext, ((CardModel)this).Owner, val, (Func<CardModel, bool>)null, (AbstractModel)(object)this))
		{
			await CardPileCmd.Add(item, (PileType)1, (CardPilePosition)2, (AbstractModel)null, false);
		}
	}

	protected override void OnUpgrade()
	{
		((DynamicVar)((CardModel)this).DynamicVars.Block).UpgradeValueBy(2m);
		((DynamicVar)((CardModel)this).DynamicVars.Cards).UpgradeValueBy(1m);
	}
}
