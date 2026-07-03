using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Danjin.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace Danjin.Powers;

public sealed class FuSuPower : DanjinPower
{
	public const decimal HealPercent = 0.10m;

	public override PowerType Type => (PowerType)1;

	public override PowerStackType StackType => (PowerStackType)1;

	public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		if (side == ((PowerModel)this).Owner.Side && ((PowerModel)this).Amount > 0 && ((PowerModel)this).Owner.IsAlive)
		{
			decimal num = Math.Floor((decimal)((PowerModel)this).Owner.MaxHp * 0.10m);
			if (num > 0m)
			{
				DanjinLog.Verbose($">>>[DanjinMod] 复苏：回合结束回复 {num} 点HP（{0.10m:P0} × MaxHp{((PowerModel)this).Owner.MaxHp}）");
				((PowerModel)this).Flash();
				await CreatureCmd.Heal(((PowerModel)this).Owner, num, true);
			}
			if (((PowerModel)this).Owner.IsAlive)
			{
				await PowerCmd.Apply<FuSuPower>(choiceContext, ((PowerModel)this).Owner, -1m, ((PowerModel)this).Owner, (CardModel)null, true);
			}
		}
	}
}
