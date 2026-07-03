using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Danjin.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace Danjin.Cards;

public class MingJingZhiShui : DanjinCard
{
	public override int CanonicalStarCost => 2;

	public override IEnumerable<CardKeyword> CanonicalKeywords
	{
		get
		{
			if (!((CardModel)this).IsUpgraded)
			{
				return Array.Empty<CardKeyword>();
			}
			return new _003C_003Ez__ReadOnlySingleElementList<CardKeyword>((CardKeyword)5);
		}
	}

	protected override HashSet<CardTag> CanonicalTags => new HashSet<CardTag> { (CardTag)0 };

	protected override IEnumerable<IHoverTip> AdditionalHoverTips => new _003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.FromCard<MingXiang>(false));

	protected override IEnumerable<DynamicVar> CanonicalVars => Array.Empty<DynamicVar>();

	public MingJingZhiShui()
		: base(2, (CardType)3, (CardRarity)4, (TargetType)1)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		Creature creature = ((CardModel)this).Owner.Creature;
		List<PowerModel> list = creature.Powers.Where((PowerModel p) => (int)p.TypeForCurrentAmount == 2).ToList();
		foreach (PowerModel item in list)
		{
			await PowerCmd.Remove(item);
		}
		await PowerCmd.Apply<MingJingZhiShuiPower>(choiceContext, creature, 1m, creature, (CardModel)(object)this, false);
		MingJingZhiShuiPower power = creature.GetPower<MingJingZhiShuiPower>();
		if (power != null)
		{
			await power.TransformExistingHandStatusCards();
		}
	}

	protected override void OnUpgrade()
	{
		((CardModel)this).AddKeyword((CardKeyword)5);
	}
}
