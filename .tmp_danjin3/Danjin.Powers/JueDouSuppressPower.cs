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

public sealed class JueDouSuppressPower : DanjinPower
{
	public override PowerType Type => (PowerType)2;

	public override PowerStackType StackType => (PowerStackType)2;

	public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		if (dealer == ((PowerModel)this).Owner && ValuePropExtensions.IsPoweredAttack(props))
		{
			return 0.7m;
		}
		return 1m;
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
