using System.Collections.Generic;
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

public class ShangKouSiLie : DanjinCard
{
	protected override HashSet<CardTag> CanonicalTags => new HashSet<CardTag> { (CardTag)0 };

	protected override IEnumerable<IHoverTip> AdditionalHoverTips => new _003C_003Ez__ReadOnlyArray<IHoverTip>((IHoverTip[])(object)new IHoverTip[2]
	{
		HoverTipFactory.FromPower<WoundPower>((int?)null),
		HoverTipFactory.FromPower<ChuXuePower>((int?)null)
	});

	protected override IEnumerable<DynamicVar> CanonicalVars => new _003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new DynamicVar("Multiplier", 1m));

	public ShangKouSiLie()
		: base(1, (CardType)3, (CardRarity)3, (TargetType)3)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		int multiplier = (int)((CardModel)this).DynamicVars["Multiplier"].BaseValue;
		if (multiplier <= 0)
		{
			return;
		}
		foreach (Creature hittableEnemy in ((CardModel)this).CombatState.HittableEnemies)
		{
			if (hittableEnemy.IsAlive)
			{
				await PowerCmd.Apply<ShangKouSiLiePower>(choiceContext, hittableEnemy, (decimal)multiplier, ((CardModel)this).Owner.Creature, (CardModel)(object)this, false);
			}
		}
	}

	protected override void OnUpgrade()
	{
		((CardModel)this).DynamicVars["Multiplier"].UpgradeValueBy(1m);
	}
}
