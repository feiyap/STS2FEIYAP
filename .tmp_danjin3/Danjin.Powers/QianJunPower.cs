using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Danjin.Powers;

public sealed class QianJunPower : DanjinPower, IStanceFreezePower
{
	public override PowerType Type => (PowerType)1;

	public override PowerStackType StackType => (PowerStackType)2;

	public bool? LockedToAttack => false;

	private int ShouShi()
	{
		Creature owner = ((PowerModel)this).Owner;
		if (owner == null)
		{
			return 0;
		}
		return owner.GetPowerAmount<ShouShiPower>();
	}

	public override decimal ModifyBlockMultiplicative(Creature target, decimal block, ValueProp props, CardModel? cardSource, CardPlay? cardPlay)
	{
		if (target == ((PowerModel)this).Owner)
		{
			return 1m + 0.25m * (decimal)ShouShi();
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
