using System.Collections.Generic;
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

public class WeiWoSuoYong : DanjinCard
{
	private const string ExtraBlockKey = "ExtraBlock";

	protected override HashSet<CardTag> CanonicalTags => new HashSet<CardTag> { (CardTag)0 };

	protected override IEnumerable<IHoverTip> AdditionalHoverTips => new _003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.FromPower<WoundPower>((int?)null));

	protected override IEnumerable<DynamicVar> CanonicalVars => new _003C_003Ez__ReadOnlyArray<DynamicVar>((DynamicVar[])(object)new DynamicVar[2]
	{
		(DynamicVar)new BlockVar(8m, (ValueProp)8),
		(DynamicVar)new BlockVar("ExtraBlock", 8m, (ValueProp)8)
	});

	public override bool GainsBlock => true;

	public WeiWoSuoYong()
		: base(1, (CardType)2, (CardRarity)3, (TargetType)1)
	{
	}

	private static bool AnyEnemyHasWound(CardModel card)
	{
		ICombatState combatState = card.CombatState;
		if (combatState == null)
		{
			return false;
		}
		foreach (Creature hittableEnemy in combatState.HittableEnemies)
		{
			if (hittableEnemy.IsAlive)
			{
				WoundPower power = hittableEnemy.GetPower<WoundPower>();
				if (((power != null && ((PowerModel)power).Amount != 0) ? 1 : 0) > (false ? 1 : 0))
				{
					return true;
				}
			}
		}
		return false;
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		BlockVar block = ((CardModel)this).DynamicVars.Block;
		if (((DynamicVar)block).BaseValue > 0m)
		{
			await CreatureCmd.GainBlock(((CardModel)this).Owner.Creature, block, cardPlay, false);
		}
		if (AnyEnemyHasWound((CardModel)(object)this))
		{
			BlockVar val = (BlockVar)((CardModel)this).DynamicVars["ExtraBlock"];
			if (((DynamicVar)val).BaseValue > 0m)
			{
				await CreatureCmd.GainBlock(((CardModel)this).Owner.Creature, val, cardPlay, false);
			}
		}
	}

	protected override void OnUpgrade()
	{
		((DynamicVar)((CardModel)this).DynamicVars.Block).UpgradeValueBy(3m);
		((CardModel)this).DynamicVars["ExtraBlock"].UpgradeValueBy(3m);
	}
}
