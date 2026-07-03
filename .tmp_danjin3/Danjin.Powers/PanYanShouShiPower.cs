using System.Threading.Tasks;
using Danjin.Resources;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Danjin.Powers;

public sealed class PanYanShouShiPower : DanjinPower
{
	public override PowerType Type => (PowerType)1;

	public override PowerStackType StackType => (PowerStackType)1;

	public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
	{
		if (player == ((PowerModel)this).Owner.Player)
		{
			await StanceCmd.GainDefenseStance(choiceContext, ((PowerModel)this).Owner.Player);
			int powerAmount = ((PowerModel)this).Owner.GetPowerAmount<ShouShiPower>();
			if (powerAmount > 0)
			{
				((PowerModel)this).Flash();
				await CreatureCmd.GainBlock(((PowerModel)this).Owner, (decimal)(powerAmount * ((PowerModel)this).Amount), (ValueProp)4, (CardPlay)null, false);
			}
		}
	}
}
