using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Danjin.Powers;
using Danjin.Variables;
using Danjin.Vfx;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace Danjin.Cards;

public class JinRan : DanjinCard
{
	public override IEnumerable<CardKeyword> CanonicalKeywords => new _003C_003Ez__ReadOnlySingleElementList<CardKeyword>(DanjinCardKeywords.FeiRen);

	protected override HashSet<CardTag> CanonicalTags => new HashSet<CardTag> { (CardTag)0 };

	protected override IEnumerable<DynamicVar> CanonicalVars => new _003C_003Ez__ReadOnlyArray<DynamicVar>((DynamicVar[])(object)new DynamicVar[4]
	{
		(DynamicVar)new JinRanDamageVar(3m),
		new DynamicVar("HitCount", 2m),
		new FeiRenVar(),
		new DynamicVar("BleedBonus", 2m)
	});

	protected override int FeirenHitCount => (int)((CardModel)this).DynamicVars["HitCount"].BaseValue;

	protected override IEnumerable<IHoverTip> AdditionalHoverTips => new _003C_003Ez__ReadOnlyArray<IHoverTip>((IHoverTip[])(object)new IHoverTip[2]
	{
		HoverTipFactory.FromPower<ChuXuePower>((int?)null),
		HoverTipFactory.FromPower<WoundPower>((int?)null)
	});

	protected override bool ShouldGlowGoldInternal
	{
		get
		{
			ICombatState combatState = ((CardModel)this).CombatState;
			if (combatState == null)
			{
				return false;
			}
			return combatState.HittableEnemies.Any((Creature e) => e.HasPower<ChuXuePower>() || e.HasPower<WoundPower>());
		}
	}

	public JinRan()
		: base(1, (CardType)1, (CardRarity)2, (TargetType)2)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, "Target");
		await BeginFeirenAttack(choiceContext);
		int num = (int)((CardModel)this).DynamicVars["HitCount"].BaseValue;
		decimal baseValue = ((DynamicVar)((CardModel)this).DynamicVars.Damage).BaseValue;
		decimal baseValue2 = ((CardModel)this).DynamicVars["BleedBonus"].BaseValue;
		NDimensionSlashVfx slashVfx = PrepareJianQiSlash(cardPlay.Target, num);
		decimal value = baseValue;
		if (cardPlay.Target.HasPower<ChuXuePower>() || cardPlay.Target.HasPower<WoundPower>())
		{
			value += baseValue2;
		}
		value = ScaleByFeiren(value);
		if (cardPlay.Target.IsAlive)
		{
			await DamageCmd.Attack(value).WithHitCount(num).FromCard((CardModel)(object)this)
				.Targeting(cardPlay.Target)
				.BeforeDamage(DanjinCard.DoSlashHook(slashVfx))
				.Execute(choiceContext);
		}
		slashVfx?.ForceComplete();
	}

	protected override void OnUpgrade()
	{
		((DynamicVar)((CardModel)this).DynamicVars.Damage).UpgradeValueBy(2m);
	}
}
