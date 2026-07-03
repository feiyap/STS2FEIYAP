using System.Threading.Tasks;
using Danjin.Resources;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Danjin.Powers;

public sealed class XiuLuoXingTaiPower : DanjinPower
{
	private int _grantedStr;

	private int _grantedDex;

	public override PowerType Type => (PowerType)1;

	public override PowerStackType StackType => (PowerStackType)1;

	public async Task Resync(PlayerChoiceContext choiceContext)
	{
		if (((PowerModel)this).Owner != null)
		{
			int powerAmount = ((PowerModel)this).Owner.GetPowerAmount<GongShiPower>();
			int powerAmount2 = ((PowerModel)this).Owner.GetPowerAmount<ShouShiPower>();
			int wantStr = powerAmount;
			int wantDex = powerAmount2;
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
		Creature owner = ((PowerModel)this).Owner;
		if (((owner != null) ? owner.Player : null) != null)
		{
			int maxStacks = StanceCmd.GetMaxStacks(((PowerModel)this).Owner.Player);
			int powerAmount = ((PowerModel)this).Owner.GetPowerAmount<GongShiPower>();
			int powerAmount2 = ((PowerModel)this).Owner.GetPowerAmount<ShouShiPower>();
			if (powerAmount > 0 && powerAmount < maxStacks)
			{
				await StanceCmd.GainAttackStance(choiceContext, ((PowerModel)this).Owner.Player, maxStacks - powerAmount);
			}
			else if (powerAmount2 > 0 && powerAmount2 < maxStacks)
			{
				await StanceCmd.GainDefenseStance(choiceContext, ((PowerModel)this).Owner.Player, maxStacks - powerAmount2);
			}
		}
		await Resync(choiceContext);
	}

	public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
	{
		if (player.Creature == ((PowerModel)this).Owner)
		{
			int maxStacks = StanceCmd.GetMaxStacks(((PowerModel)this).Owner.Player);
			int powerAmount = ((PowerModel)this).Owner.GetPowerAmount<GongShiPower>();
			int powerAmount2 = ((PowerModel)this).Owner.GetPowerAmount<ShouShiPower>();
			if (powerAmount > 0 && powerAmount < maxStacks)
			{
				await StanceCmd.GainAttackStance(choiceContext, ((PowerModel)this).Owner.Player, maxStacks - powerAmount);
			}
			else if (powerAmount2 > 0 && powerAmount2 < maxStacks)
			{
				await StanceCmd.GainDefenseStance(choiceContext, ((PowerModel)this).Owner.Player, maxStacks - powerAmount2);
			}
			await Resync(choiceContext);
		}
	}
}
