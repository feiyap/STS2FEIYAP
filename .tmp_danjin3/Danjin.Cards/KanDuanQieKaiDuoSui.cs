using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Danjin.Extensions;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace Danjin.Cards;

public class KanDuanQieKaiDuoSui : DanjinCard
{
	public override IEnumerable<CardKeyword> CanonicalKeywords
	{
		get
		{
			if (!((CardModel)this).IsUpgraded)
			{
				return new _003C_003Ez__ReadOnlySingleElementList<CardKeyword>((CardKeyword)1);
			}
			return Array.Empty<CardKeyword>();
		}
	}

	public override int CanonicalStarCost => 6;

	protected override HashSet<CardTag> CanonicalTags => new HashSet<CardTag> { (CardTag)0 };

	protected override IEnumerable<IHoverTip> AdditionalHoverTips => new _003C_003Ez__ReadOnlySingleElementList<IHoverTip>(RawHoverTipBuilder.Build("replay-tip", "重放", "这张牌打出后会额外再打出一次。"));

	protected override IEnumerable<DynamicVar> CanonicalVars => Array.Empty<DynamicVar>();

	public KanDuanQieKaiDuoSui()
		: base(0, (CardType)2, (CardRarity)4, (TargetType)1)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		CardPile pile = PileTypeExtensions.GetPile((PileType)2, ((CardModel)this).Owner);
		if (pile == null)
		{
			return;
		}
		List<CardModel> list = pile.Cards.Where((CardModel c) => (int)c.Type == 1 && c.BaseReplayCount == 0).ToList();
		if (list.Count != 0)
		{
			CardSelectorPrefs val = default(CardSelectorPrefs);
			((CardSelectorPrefs)(ref val))._002Ector(((CardModel)this).SelectionScreenPrompt, 1);
			CardModel val2 = (await CardSelectCmd.FromSimpleGrid(choiceContext, (IReadOnlyList<CardModel>)list, ((CardModel)this).Owner, val)).FirstOrDefault();
			if (val2 != null)
			{
				val2.BaseReplayCount += 2;
			}
		}
	}

	protected override void OnUpgrade()
	{
		((CardModel)this).RemoveKeyword((CardKeyword)1);
	}
}
