using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Danjin.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace Danjin.Cards;

public class GongMingBaoFa : DanjinCard
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

	protected override HashSet<CardTag> CanonicalTags => new HashSet<CardTag> { (CardTag)0 };

	protected override IEnumerable<DynamicVar> CanonicalVars => Array.Empty<DynamicVar>();

	public GongMingBaoFa()
		: base(1, (CardType)2, (CardRarity)4, (TargetType)1)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await PowerCmd.Apply<GongMingBaoFaPower>(choiceContext, ((CardModel)this).Owner.Creature, 1m, ((CardModel)this).Owner.Creature, (CardModel)(object)this, false);
		GongMingBaoFaPower power = ((CardModel)this).Owner.Creature.GetPower<GongMingBaoFaPower>();
		if (power == null)
		{
			return;
		}
		foreach (CardModel card in PileTypeExtensions.GetPile((PileType)2, ((CardModel)this).Owner).Cards)
		{
			power.MarkIfEligible(card);
		}
	}

	protected override void OnUpgrade()
	{
		((CardModel)this).RemoveKeyword((CardKeyword)1);
	}
}
