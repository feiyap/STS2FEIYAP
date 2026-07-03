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
using MegaCrit.Sts2.Core.ValueProps;

namespace Danjin.Cards;

public class JieCuo : DanjinCard
{
	public override IEnumerable<CardKeyword> CanonicalKeywords => Array.Empty<CardKeyword>();

	protected override HashSet<CardTag> CanonicalTags => new HashSet<CardTag> { (CardTag)0 };

	protected override IEnumerable<IHoverTip> AdditionalHoverTips => new _003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.FromPower<WoundPower>((int?)null));

	protected override IEnumerable<DynamicVar> CanonicalVars => new _003C_003Ez__ReadOnlySingleElementList<DynamicVar>((DynamicVar)new DamageVar(15m, (ValueProp)8));

	protected override bool IsPlayable
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
				WoundPower power = e.GetPower<WoundPower>();
				return ((power != null && ((PowerModel)power).Amount != 0) ? 1 : 0) > (false ? 1 : 0);
			});
		}
	}

	protected override bool ShouldGlowGoldInternal => ((CardModel)this).IsPlayable;

	public JieCuo()
		: base(0, (CardType)1, (CardRarity)4, (TargetType)2)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, "Target");
		WoundPower power = cardPlay.Target.GetPower<WoundPower>();
		if (((power != null && ((PowerModel)power).Amount != 0) ? 1 : 0) > (false ? 1 : 0) && cardPlay.Target.IsAlive)
		{
			PrepareJianQiSlash(cardPlay.Target)?.DoSlash();
			await CreatureCmd.Damage(choiceContext, cardPlay.Target, ((DynamicVar)((CardModel)this).DynamicVars.Damage).BaseValue, (ValueProp)10, ((CardModel)this).Owner.Creature, (CardModel)(object)this);
		}
	}

	protected override void OnUpgrade()
	{
		((DynamicVar)((CardModel)this).DynamicVars.Damage).UpgradeValueBy(5m);
	}
}
