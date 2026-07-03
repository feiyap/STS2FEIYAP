using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Danjin.Powers;

public sealed class XueWuMiManPower : DanjinPower
{
	public override PowerType Type => (PowerType)1;

	public override PowerStackType StackType => (PowerStackType)2;

	public override async Task AfterDamageGiven(PlayerChoiceContext choiceContext, Creature? dealer, DamageResult result, ValueProp props, Creature target, CardModel? cardSource)
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		if (dealer != ((PowerModel)this).Owner || !ValuePropExtensions.IsPoweredAttack(props) || result.TotalDamage <= 0)
		{
			return;
		}
		ICombatState combatState = ((PowerModel)this).Owner.CombatState;
		if (combatState == null)
		{
			return;
		}
		((PowerModel)this).Flash();
		foreach (Creature hittableEnemy in combatState.HittableEnemies)
		{
			if (hittableEnemy.IsAlive)
			{
				await PowerCmd.Apply<ChuXuePower>(choiceContext, hittableEnemy, (decimal)((PowerModel)this).Amount, ((PowerModel)this).Owner, (CardModel)null, false);
			}
		}
	}

	public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		if (side == ((PowerModel)this).Owner.Side)
		{
			await PowerCmd.Remove((PowerModel)(object)this);
		}
	}
}
