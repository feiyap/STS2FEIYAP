using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Danjin.Audio;
using Danjin.Resources;
using Danjin.Utils;
using Danjin.Variables;
using Danjin.Vfx;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Danjin.Cards;

public class JianQi : DanjinCard
{
	private static ModSound? _voice;

	public override IEnumerable<CardKeyword> CanonicalKeywords => new _003C_003Ez__ReadOnlyArray<CardKeyword>((CardKeyword[])(object)new CardKeyword[2]
	{
		(CardKeyword)(int)DanjinCardKeywords.FeiRen,
		(CardKeyword)3
	});

	protected override HashSet<CardTag> CanonicalTags => new HashSet<CardTag> { (CardTag)0 };

	protected override IEnumerable<DynamicVar> CanonicalVars => new _003C_003Ez__ReadOnlyArray<DynamicVar>((DynamicVar[])(object)new DynamicVar[3]
	{
		(DynamicVar)new DamageVar(4m, (ValueProp)8),
		new DynamicVar("HitCount", 2m),
		new FeiRenVar()
	});

	protected override int FeirenHitCount => (int)((CardModel)this).DynamicVars["HitCount"].BaseValue;

	public JianQi()
		: base(1, (CardType)1, (CardRarity)1, (TargetType)2)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		DanjinVoice.Play(_voice ?? (_voice = new ModSound("res://Danjin/Sounds/Cards/jian_qi.ogg")));
		await BeginFeirenAttack(choiceContext);
		int num = (int)((CardModel)this).DynamicVars["HitCount"].BaseValue;
		decimal num2 = ScaleByFeiren(((DynamicVar)((CardModel)this).DynamicVars.Damage).BaseValue);
		NDimensionSlashVfx slashVfx = PrepareJianQiSlash(cardPlay.Target, num);
		if (cardPlay.Target != null && cardPlay.Target.IsAlive)
		{
			await DamageCmd.Attack(num2).WithHitCount(num).FromCard((CardModel)(object)this)
				.Targeting(cardPlay.Target)
				.BeforeDamage(DanjinCard.DoSlashHook(slashVfx))
				.Execute(choiceContext);
		}
		slashVfx?.ForceComplete();
		if (!CombatManager.Instance.History.CardPlaysFinished.Any((CardPlayFinishedEntry e) => e.CardPlay.Card.Owner == ((CardModel)this).Owner && e.CardPlay.Card is JianQi))
		{
			await TonghuaCmd.GainTonghua(1m, ((CardModel)this).Owner);
		}
	}

	protected override void OnUpgrade()
	{
		((DynamicVar)((CardModel)this).DynamicVars.Damage).UpgradeValueBy(2m);
	}
}
