using System.Collections.Generic;
using System.Threading.Tasks;
using Danjin.Resources;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace Danjin.Cards;

public class BingXiNingShen : DanjinCard
{
	public override IEnumerable<CardKeyword> CanonicalKeywords => new _003C_003Ez__ReadOnlySingleElementList<CardKeyword>(DanjinCardKeywords.LiuZhuan);

	public override int CanonicalStarCost => 2;

	protected override HashSet<CardTag> CanonicalTags => new HashSet<CardTag> { (CardTag)0 };

	protected override IEnumerable<DynamicVar> CanonicalVars => new _003C_003Ez__ReadOnlySingleElementList<DynamicVar>((DynamicVar)new EnergyVar(2));

	public BingXiNingShen()
		: base(0, (CardType)2, (CardRarity)3, (TargetType)1)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await StanceCmd.SwitchStance(choiceContext, ((CardModel)this).Owner);
		await PlayerCmd.GainEnergy((decimal)(int)((DynamicVar)((CardModel)this).DynamicVars.Energy).BaseValue, ((CardModel)this).Owner);
	}

	protected override void OnUpgrade()
	{
		((DynamicVar)((CardModel)this).DynamicVars.Energy).UpgradeValueBy(1m);
	}
}
