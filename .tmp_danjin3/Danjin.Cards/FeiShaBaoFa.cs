using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Danjin.Cards;

public class FeiShaBaoFa : DanjinTokenCard
{
	public override IEnumerable<CardKeyword> CanonicalKeywords => new _003C_003Ez__ReadOnlySingleElementList<CardKeyword>(DanjinCardKeywords.LianDuan);

	protected override HashSet<CardTag> CanonicalTags => new HashSet<CardTag> { (CardTag)0 };

	protected override IEnumerable<DynamicVar> CanonicalVars => new _003C_003Ez__ReadOnlySingleElementList<DynamicVar>((DynamicVar)new DamageVar(8m, (ValueProp)8));

	public FeiShaBaoFa()
		: base(0, (CardType)1, (TargetType)3)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		decimal baseValue = ((DynamicVar)((CardModel)this).DynamicVars.Damage).BaseValue;
		AoeJianQiSlashGroup slashVfx = PrepareAoeJianQiSlashRandom(1);
		await DamageCmd.Attack(baseValue).FromCard((CardModel)(object)this).TargetingAllOpponents(((CardModel)this).CombatState)
			.BeforeDamage(DanjinCard.DoSlashHook(slashVfx))
			.Execute(choiceContext);
		slashVfx?.ForceComplete();
	}

	protected override void OnUpgrade()
	{
	}
}
