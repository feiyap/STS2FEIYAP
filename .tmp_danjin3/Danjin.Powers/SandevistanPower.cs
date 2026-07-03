using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace Danjin.Powers;

public sealed class SandevistanPower : DanjinPower
{
	public override PowerType Type => (PowerType)1;

	public override PowerStackType StackType => (PowerStackType)1;

	private static bool IsPoweredAttack(ValueProp props)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		if (((Enum)props).HasFlag((Enum)(object)(ValueProp)8))
		{
			return !((Enum)props).HasFlag((Enum)(object)(ValueProp)4);
		}
		return false;
	}

	public override decimal ModifyHpLostAfterOstyLate(Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		if (target != ((PowerModel)this).Owner)
		{
			return amount;
		}
		if (dealer == null || !dealer.IsEnemy)
		{
			return amount;
		}
		if (!IsPoweredAttack(props))
		{
			return amount;
		}
		if (amount > 0m)
		{
			((PowerModel)this).Flash();
		}
		return 0m;
	}

	public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		if ((int)side == 2)
		{
			await PowerCmd.TickDownDuration((PowerModel)(object)this);
		}
	}

	public override async Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
	{
		if ((object)power == this && ((PowerModel)this).Amount == 0)
		{
			Creature owner = ((PowerModel)this).Owner;
			if (owner != null && owner.IsAlive)
			{
				await PowerCmd.Apply<StrengthPower>(choiceContext, owner, -3m, owner, (CardModel)null, false);
				await PowerCmd.Apply<DexterityPower>(choiceContext, owner, -3m, owner, (CardModel)null, false);
			}
		}
	}
}
