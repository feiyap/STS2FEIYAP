using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Danjin.Powers;

public sealed class JianYingPower : DanjinPower
{
	public override PowerType Type => (PowerType)2;

	public override PowerStackType StackType => (PowerStackType)1;

	public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		if (side == ((PowerModel)this).Owner.Side)
		{
			if (((PowerModel)this).Amount <= 0)
			{
				await PowerCmd.Remove((PowerModel)(object)this);
				return;
			}
			((PowerModel)this).Flash();
			int amount = ((PowerModel)this).Amount;
			await PowerCmd.Apply<StrengthPower>(choiceContext, ((PowerModel)this).Owner, (decimal)amount, (Creature)null, (CardModel)null, false);
			await PowerCmd.Remove((PowerModel)(object)this);
		}
	}
}
