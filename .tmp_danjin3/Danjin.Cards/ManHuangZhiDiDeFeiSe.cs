using System.Collections.Generic;
using System.Threading.Tasks;
using Danjin.Powers;
using Danjin.Resources;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace Danjin.Cards;

public class ManHuangZhiDiDeFeiSe : DanjinCard
{
	protected override HashSet<CardTag> CanonicalTags => new HashSet<CardTag> { (CardTag)0 };

	protected override IEnumerable<DynamicVar> CanonicalVars => new _003C_003Ez__ReadOnlyArray<DynamicVar>((DynamicVar[])(object)new DynamicVar[2]
	{
		new DynamicVar("CapReduction", 1m),
		(DynamicVar)new EnergyVar(1)
	});

	public ManHuangZhiDiDeFeiSe()
		: base(1, (CardType)3, (CardRarity)4, (TargetType)1)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await PowerCmd.Apply<ManHuangZhiDiDeFeiSePower>(choiceContext, ((CardModel)this).Owner.Creature, 1m, ((CardModel)this).Owner.Creature, (CardModel)(object)this, false);
		ManHuangZhiDiDeFeiSePower power = ((CardModel)this).Owner.Creature.GetPower<ManHuangZhiDiDeFeiSePower>();
		if (power != null)
		{
			power.TotalEnergyBonus += (int)((CardModel)this).DynamicVars["Energy"].BaseValue;
		}
		if (StanceCmd.GetMaxStacks(((CardModel)this).Owner) <= 0)
		{
			if (((CardModel)this).Owner.Creature.GetPowerAmount<GongShiPower>() > 0)
			{
				await PowerCmd.Remove<GongShiPower>(((CardModel)this).Owner.Creature);
			}
			if (((CardModel)this).Owner.Creature.GetPowerAmount<ShouShiPower>() > 0)
			{
				await PowerCmd.Remove<ShouShiPower>(((CardModel)this).Owner.Creature);
			}
		}
	}

	protected override void OnUpgrade()
	{
		((CardModel)this).DynamicVars["Energy"].UpgradeValueBy(1m);
	}
}
