using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Danjin.Powers;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace Danjin.Cards;

public class XueLiuBuZhi : DanjinCard
{
	public override IEnumerable<CardKeyword> CanonicalKeywords => Array.Empty<CardKeyword>();

	protected override HashSet<CardTag> CanonicalTags => new HashSet<CardTag> { (CardTag)0 };

	protected override IEnumerable<IHoverTip> AdditionalHoverTips => new _003C_003Ez__ReadOnlyArray<IHoverTip>((IHoverTip[])(object)new IHoverTip[2]
	{
		HoverTipFactory.FromPower<ChuXuePower>((int?)null),
		HoverTipFactory.FromPower<WoundPower>((int?)null)
	});

	protected override IEnumerable<DynamicVar> CanonicalVars => new _003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new DynamicVar("Bleed", 9m));

	protected override bool ShouldGlowGoldInternal
	{
		get
		{
			ICombatState combatState = ((CardModel)this).CombatState;
			if (combatState == null)
			{
				return false;
			}
			return combatState.HittableEnemies.Any(delegate(Creature e)
			{
				ChuXuePower power = e.GetPower<ChuXuePower>();
				if (((power != null && ((PowerModel)power).Amount != 0) ? 1 : 0) <= (false ? 1 : 0))
				{
					WoundPower power2 = e.GetPower<WoundPower>();
					return ((power2 != null && ((PowerModel)power2).Amount != 0) ? 1 : 0) > (false ? 1 : 0);
				}
				return true;
			});
		}
	}

	public XueLiuBuZhi()
		: base(1, (CardType)2, (CardRarity)3, (TargetType)2)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, "Target");
		if (cardPlay.Target.IsAlive)
		{
			Creature target = cardPlay.Target;
			ChuXuePower power = target.GetPower<ChuXuePower>();
			int num;
			if (((power != null && ((PowerModel)power).Amount != 0) ? 1 : 0) <= (false ? 1 : 0))
			{
				WoundPower power2 = target.GetPower<WoundPower>();
				num = ((((power2 != null && ((PowerModel)power2).Amount != 0) ? 1 : 0) > (false ? 1 : 0)) ? 1 : 0);
			}
			else
			{
				num = 1;
			}
			if (num != 0)
			{
				int num2 = (int)((CardModel)this).DynamicVars["Bleed"].BaseValue;
				await PowerCmd.Apply<ChuXuePower>(choiceContext, target, (decimal)num2, ((CardModel)this).Owner.Creature, (CardModel)(object)this, false);
			}
		}
	}

	protected override void OnUpgrade()
	{
		((CardModel)this).DynamicVars["Bleed"].UpgradeValueBy(3m);
	}
}
