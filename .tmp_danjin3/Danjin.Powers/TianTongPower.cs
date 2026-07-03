using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Danjin.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Danjin.Powers;

public sealed class TianTongPower : DanjinPower
{
	public override PowerType Type => (PowerType)1;

	public override PowerStackType StackType => (PowerStackType)1;

	public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		if (side != ((PowerModel)this).Owner.Side || ((PowerModel)this).Amount <= 0 || !((PowerModel)this).Owner.IsAlive)
		{
			return;
		}
		ICombatState combatState = ((PowerModel)this).Owner.CombatState;
		if (combatState == null)
		{
			return;
		}
		IReadOnlyList<Creature> hittableEnemies = combatState.HittableEnemies;
		if (hittableEnemies == null)
		{
			return;
		}
		List<Creature> list = hittableEnemies.Where((Creature e) => e.IsAlive).ToList();
		if (list.Count == 0)
		{
			return;
		}
		Player player = ((PowerModel)this).Owner.Player;
		if (player == null)
		{
			return;
		}
		((PowerModel)this).Flash();
		Creature val = player.RunState.Rng.CombatTargets.NextItem<Creature>((IEnumerable<Creature>)list);
		if (val != null)
		{
			int steal = ((PowerModel)this).Amount;
			DanjinLog.Verbose($">>>[DanjinMod] 天童：从 {val} 偷取 {steal} 点力量");
			await PowerCmd.Apply<StrengthPower>(choiceContext, val, (decimal)(-steal), ((PowerModel)this).Owner, (CardModel)null, false);
			if (((PowerModel)this).Owner.IsAlive)
			{
				await PowerCmd.Apply<StrengthPower>(choiceContext, ((PowerModel)this).Owner, (decimal)steal, ((PowerModel)this).Owner, (CardModel)null, false);
			}
		}
	}
}
