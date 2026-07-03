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

public sealed class XieGongShouZhenPower : DanjinPower
{
	public override PowerType Type => (PowerType)1;

	public override PowerStackType StackType => (PowerStackType)1;

	public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		if (side == ((PowerModel)this).Owner.Side && ((PowerModel)this).Amount > 0)
		{
			int powerAmount = ((PowerModel)this).Owner.GetPowerAmount<GongShiPower>();
			if (powerAmount > 0)
			{
				((PowerModel)this).Flash();
				await CreatureCmd.GainBlock(((PowerModel)this).Owner, (decimal)(powerAmount * ((PowerModel)this).Amount), (ValueProp)4, (CardPlay)null, false);
			}
		}
	}
}
