using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Danjin.Extensions;
using Danjin.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Danjin.Cards;

public class HeavySlash : DanjinCard
{
	protected override HashSet<CardTag> CanonicalTags => new HashSet<CardTag> { (CardTag)0 };

	protected override IEnumerable<IHoverTip> AdditionalHoverTips => new _003C_003Ez__ReadOnlyArray<IHoverTip>((IHoverTip[])(object)new IHoverTip[2]
	{
		HoverTipFactory.FromPower<ChuXuePower>((int?)null),
		RawHoverTipBuilder.Build("danjin-stance-atk", "攻势1", "打出时需要至少[blue]1[/blue]层[red]攻势[/red]，否则该效果不生效。")
	});

	protected override IEnumerable<DynamicVar> CanonicalVars => new _003C_003Ez__ReadOnlyArray<DynamicVar>((DynamicVar[])(object)new DynamicVar[3]
	{
		(DynamicVar)new DamageVar(4m, (ValueProp)8),
		new DynamicVar("HitCount", 3m),
		(DynamicVar)new PowerVar<ChuXuePower>(1m)
	});

	public HeavySlash()
		: base(2, (CardType)1, (CardRarity)2, (TargetType)3)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		int num = (int)((CardModel)this).DynamicVars["HitCount"].BaseValue;
		decimal baseValue = ((DynamicVar)((CardModel)this).DynamicVars.Damage).BaseValue;
		decimal bleedAmount = ((CardModel)this).DynamicVars["ChuXuePower"].BaseValue;
		Creature creature = ((CardModel)this).Owner.Creature;
		int num2 = ((creature != null) ? creature.GetPowerAmount<GongShiPower>() : 0);
		bool bleedOn = num2 >= 1;
		AoeJianQiSlashGroup slashVfx = PrepareAoeJianQiSlashRandom(num);
		await DamageCmd.Attack(baseValue).WithHitCount(num).FromCard((CardModel)(object)this)
			.TargetingAllOpponents(((CardModel)this).CombatState)
			.BeforeDamage((Func<Task>)async delegate
			{
				slashVfx?.DoSlash();
				if (bleedOn)
				{
					await PowerCmd.Apply<ChuXuePower>(choiceContext, (IEnumerable<Creature>)((CardModel)this).CombatState.HittableEnemies, bleedAmount, ((CardModel)this).Owner.Creature, (CardModel)(object)this, false);
				}
			})
			.Execute(choiceContext);
		slashVfx?.ForceComplete();
	}

	protected override void OnUpgrade()
	{
		((DynamicVar)((CardModel)this).DynamicVars.Damage).UpgradeValueBy(1m);
	}
}
