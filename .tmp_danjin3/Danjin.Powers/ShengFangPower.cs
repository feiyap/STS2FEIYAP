using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace Danjin.Powers;

public sealed class ShengFangPower : DanjinPower
{
	private const int BleedOnTrigger = 1;

	public override PowerType Type => (PowerType)1;

	public override PowerStackType StackType => (PowerStackType)1;

	public override async Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
	{
		if (applier != ((PowerModel)this).Owner || amount <= 0m || !(power is ZhuShiZhiKePower) || power.Owner == null || !power.Owner.IsAlive || !power.Owner.IsEnemy)
		{
			return;
		}
		((PowerModel)this).Flash();
		if (((PowerModel)this).Amount > 0)
		{
			Player player = ((PowerModel)this).Owner.Player;
			if (player != null)
			{
				await CardPileCmd.Draw((PlayerChoiceContext)new BlockingPlayerChoiceContext(), (decimal)((PowerModel)this).Amount, player, false);
			}
		}
		await PowerCmd.Apply<ChuXuePower>(choiceContext, power.Owner, 1m, ((PowerModel)this).Owner, (CardModel)null, false);
	}
}
