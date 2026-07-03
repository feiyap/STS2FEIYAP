using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.CommonUi;

namespace Danjin.Cards;

public class TianDianBao : DanjinCard
{
	protected override HashSet<CardTag> CanonicalTags => new HashSet<CardTag> { (CardTag)0 };

	protected override IEnumerable<DynamicVar> CanonicalVars => Array.Empty<DynamicVar>();

	protected override IEnumerable<IHoverTip> AdditionalHoverTips
	{
		get
		{
			yield return HoverTipFactory.FromCard<XingXingSu>(((CardModel)this).IsUpgraded);
		}
	}

	public TianDianBao()
		: base(1, (CardType)2, (CardRarity)3, (TargetType)1)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		CardPile pile = PileTypeExtensions.GetPile((PileType)1, ((CardModel)this).Owner);
		if (pile == null || pile.Cards.Count == 0)
		{
			return;
		}
		int num = Math.Min(2, pile.Cards.Count);
		CardSelectorPrefs val = default(CardSelectorPrefs);
		((CardSelectorPrefs)(ref val))._002Ector(((CardModel)this).SelectionScreenPrompt, num);
		foreach (CardModel item in await CardSelectCmd.FromSimpleGrid(choiceContext, pile.Cards, ((CardModel)this).Owner, val))
		{
			XingXingSu xingXingSu = ((CardModel)this).CombatState.CreateCard<XingXingSu>(((CardModel)this).Owner);
			if (((CardModel)this).IsUpgraded)
			{
				CardCmd.Upgrade((CardModel)(object)xingXingSu, (CardPreviewStyle)1);
			}
			await CardCmd.Transform(item, (CardModel)(object)xingXingSu, (CardPreviewStyle)1);
		}
	}

	protected override void OnUpgrade()
	{
	}
}
