using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Danjin.Powers;

public sealed class MaiChongHuDunPower : DanjinPower
{
	public override PowerType Type => (PowerType)1;

	public override PowerStackType StackType => (PowerStackType)1;

	public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		if (side != ((PowerModel)this).Owner.Side || ((PowerModel)this).Amount <= 0)
		{
			return;
		}
		int powerAmount = ((PowerModel)this).Owner.GetPowerAmount<ShouShiPower>();
		if (powerAmount > 0)
		{
			ICombatState combatState = ((PowerModel)this).CombatState;
			List<Creature> list = ((combatState != null) ? combatState.HittableEnemies.Where((Creature e) => e.IsAlive).ToList() : null);
			if (list != null && list.Count != 0)
			{
				((PowerModel)this).Flash();
				await CreatureCmd.Damage(choiceContext, (IEnumerable<Creature>)list, (decimal)(powerAmount * ((PowerModel)this).Amount), (ValueProp)4, ((PowerModel)this).Owner);
			}
		}
	}
}
