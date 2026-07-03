using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Danjin.Cards;

public class BasicAttack : DanjinCard
{
	protected override HashSet<CardTag> CanonicalTags => new HashSet<CardTag> { (CardTag)1 };

	protected override IEnumerable<DynamicVar> CanonicalVars => new _003C_003Ez__ReadOnlySingleElementList<DynamicVar>((DynamicVar)new DamageVar(6m, (ValueProp)8));

	public BasicAttack()
		: base(1, (CardType)1, (CardRarity)1, (TargetType)2)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await PlayJianQiAttackOnce(choiceContext, cardPlay.Target);
	}

	protected override void OnUpgrade()
	{
		((DynamicVar)((CardModel)this).DynamicVars.Damage).UpgradeValueBy(3m);
	}
}
