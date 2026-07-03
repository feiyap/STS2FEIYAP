using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace Danjin.Powers;

public sealed class YinHongMiWuPower : DanjinPower
{
	public override PowerType Type => (PowerType)1;

	public override PowerStackType StackType => (PowerStackType)1;

	public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		if (side != ((PowerModel)this).Owner.Side || ((PowerModel)this).Amount <= 0)
		{
			return;
		}
		((PowerModel)this).Flash();
		foreach (Creature hittableEnemy in combatState.HittableEnemies)
		{
			if (hittableEnemy.IsAlive)
			{
				await PowerCmd.Apply<ChuXuePower>((PlayerChoiceContext)new BlockingPlayerChoiceContext(), hittableEnemy, (decimal)((PowerModel)this).Amount, ((PowerModel)this).Owner, (CardModel)null, false);
			}
		}
	}
}
