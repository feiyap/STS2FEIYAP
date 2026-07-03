using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Danjin.Powers;

public sealed class ZhenCiWuZhuangPower : DanjinPower
{
	public override PowerType Type => (PowerType)1;

	public override PowerStackType StackType => (PowerStackType)1;

	public override async Task AfterBlockGained(Creature creature, decimal amount, ValueProp props, CardModel? cardSource)
	{
		if (creature != ((PowerModel)this).Owner || amount <= 0m)
		{
			return;
		}
		ICombatState combatState = ((PowerModel)this).Owner.CombatState;
		if (combatState == null)
		{
			return;
		}
		IReadOnlyList<Creature> hittableEnemies = combatState.HittableEnemies;
		if (hittableEnemies.Count == 0)
		{
			return;
		}
		Player player = ((PowerModel)this).Owner.Player;
		if (player != null)
		{
			Creature val = player.RunState.Rng.CombatTargets.NextItem<Creature>((IEnumerable<Creature>)hittableEnemies);
			if (val != null && val.IsAlive)
			{
				((PowerModel)this).Flash();
				await PowerCmd.Apply<ChuXuePower>((PlayerChoiceContext)new BlockingPlayerChoiceContext(), val, (decimal)((PowerModel)this).Amount, ((PowerModel)this).Owner, (CardModel)null, false);
			}
		}
	}
}
