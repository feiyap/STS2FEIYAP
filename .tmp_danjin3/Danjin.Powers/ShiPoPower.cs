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

public sealed class ShiPoPower : DanjinPower
{
	public override PowerType Type => (PowerType)1;

	public override PowerStackType StackType => (PowerStackType)1;

	private bool IsEnemyAttack(Creature target, Creature? dealer)
	{
		if (dealer == null)
		{
			return false;
		}
		if (dealer == target)
		{
			return false;
		}
		return dealer.Monster != null;
	}

	public override decimal ModifyHpLostAfterOstyLate(Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
	{
		if (target != ((PowerModel)this).Owner)
		{
			return amount;
		}
		if (amount <= 0m)
		{
			return amount;
		}
		if (!IsEnemyAttack(target, dealer))
		{
			return amount;
		}
		return 0m;
	}

	public override async Task AfterModifyingHpLostAfterOsty()
	{
		await PowerCmd.Decrement((PowerModel)(object)this);
	}

	public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		if (side != ((PowerModel)this).Owner.Side && ((PowerModel)this).Amount > 0)
		{
			await PowerCmd.Apply<ShiPoPower>(choiceContext, ((PowerModel)this).Owner, -1m, ((PowerModel)this).Owner, (CardModel)null, true);
		}
	}
}
