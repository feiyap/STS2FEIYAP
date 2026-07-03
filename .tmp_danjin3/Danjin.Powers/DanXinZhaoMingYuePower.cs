using System.Threading.Tasks;
using Danjin.Resources;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace Danjin.Powers;

public sealed class DanXinZhaoMingYuePower : DanjinPower
{
	public override PowerType Type => (PowerType)1;

	public override PowerStackType StackType => (PowerStackType)1;

	public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
	{
		if (player == ((PowerModel)this).Owner.Player)
		{
			((PowerModel)this).Flash();
			await TonghuaCmd.GainTonghua(((PowerModel)this).Amount, player);
		}
	}
}
