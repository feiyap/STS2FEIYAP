using System.Collections.Generic;
using System.Threading.Tasks;
using Danjin.Audio;
using Danjin.Utils;
using Danjin.Variables;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Danjin.Cards;

public class MingDuan : DanjinCard
{
	private static ModSound? _voice;

	public override IEnumerable<CardKeyword> CanonicalKeywords => new _003C_003Ez__ReadOnlySingleElementList<CardKeyword>(DanjinCardKeywords.FeiRen);

	protected override HashSet<CardTag> CanonicalTags => new HashSet<CardTag> { (CardTag)0 };

	protected override IEnumerable<IHoverTip> AdditionalHoverTips => new _003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.FromPower<ZhuShiZhiKePower>((int?)null));

	protected override IEnumerable<DynamicVar> CanonicalVars => new _003C_003Ez__ReadOnlyArray<DynamicVar>((DynamicVar[])(object)new DynamicVar[3]
	{
		(DynamicVar)new DamageVar(5m, (ValueProp)8),
		new FeiRenVar(),
		(DynamicVar)new PowerVar<ZhuShiZhiKePower>(1m)
	});

	protected override int FeirenHitCount => 1;

	public MingDuan()
		: base(1, (CardType)1, (CardRarity)2, (TargetType)3)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		DanjinVoice.Play(_voice ?? (_voice = new ModSound("res://Danjin/Sounds/Cards/ming_duan.ogg")));
		await BeginFeirenAttack(choiceContext);
		decimal num = ScaleByFeiren(((DynamicVar)((CardModel)this).DynamicVars.Damage).BaseValue);
		decimal zhuShi = ScaleByFeiren(((CardModel)this).DynamicVars["ZhuShiZhiKePower"].BaseValue);
		AoeJianQiSlashGroup slashVfx = PrepareAoeJianQiSlashRandom(1);
		await DamageCmd.Attack(num).FromCard((CardModel)(object)this).TargetingAllOpponents(((CardModel)this).CombatState)
			.BeforeDamage(DanjinCard.DoSlashHook(slashVfx))
			.Execute(choiceContext);
		slashVfx?.ForceComplete();
		if (zhuShi > 0m && ((CardModel)this).CombatState != null)
		{
			await PowerCmd.Apply<ZhuShiZhiKePower>(choiceContext, (IEnumerable<Creature>)((CardModel)this).CombatState.HittableEnemies, zhuShi, ((CardModel)this).Owner.Creature, (CardModel)(object)this, false);
		}
	}

	protected override void OnUpgrade()
	{
		((DynamicVar)((CardModel)this).DynamicVars.Damage).UpgradeValueBy(3m);
	}
}
