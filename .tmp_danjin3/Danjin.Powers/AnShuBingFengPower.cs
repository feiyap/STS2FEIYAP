using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Danjin.Powers;

public sealed class AnShuBingFengPower : DanjinPower
{
	private int _grantedStr;

	private int _grantedDex;

	public override PowerType Type => (PowerType)1;

	public override PowerStackType StackType => (PowerStackType)1;

	public async Task Resync(PlayerChoiceContext choiceContext)
	{
		if (((PowerModel)this).Owner != null)
		{
			int amount = ((PowerModel)this).Amount;
			int powerAmount = ((PowerModel)this).Owner.GetPowerAmount<GongShiPower>();
			int powerAmount2 = ((PowerModel)this).Owner.GetPowerAmount<ShouShiPower>();
			int wantStr = ((powerAmount > 0) ? amount : 0);
			int wantDex = ((powerAmount2 > 0) ? amount : 0);
			int num = wantStr - _grantedStr;
			if (num != 0)
			{
				await PowerCmd.Apply<StrengthPower>(choiceContext, ((PowerModel)this).Owner, (decimal)num, ((PowerModel)this).Owner, (CardModel)null, true);
				_grantedStr = wantStr;
			}
			int num2 = wantDex - _grantedDex;
			if (num2 != 0)
			{
				await PowerCmd.Apply<DexterityPower>(choiceContext, ((PowerModel)this).Owner, (decimal)num2, ((PowerModel)this).Owner, (CardModel)null, true);
				_grantedDex = wantDex;
			}
		}
	}

	public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await Resync(choiceContext);
	}

	public override async Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
	{
		if ((power is GongShiPower || power is ShouShiPower) ? true : false)
		{
			await Resync(choiceContext);
		}
	}
}
