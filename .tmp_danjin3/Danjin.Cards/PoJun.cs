using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Danjin.Extensions;
using Danjin.Powers;
using Danjin.Resources;
using Danjin.Variables;
using Danjin.Vfx;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace Danjin.Cards;

public class PoJun : DanjinCard
{
	public override IEnumerable<CardKeyword> CanonicalKeywords => new _003C_003Ez__ReadOnlyArray<CardKeyword>((CardKeyword[])(object)new CardKeyword[2]
	{
		(CardKeyword)(int)DanjinCardKeywords.FeiRen,
		(CardKeyword)(int)DanjinCardKeywords.CanXin
	});

	protected override HashSet<CardTag> CanonicalTags => new HashSet<CardTag> { (CardTag)0 };

	protected override IEnumerable<IHoverTip> AdditionalHoverTips => new _003C_003Ez__ReadOnlyArray<IHoverTip>((IHoverTip[])(object)new IHoverTip[4]
	{
		HoverTipFactory.FromPower<StrengthPower>((int?)null),
		HoverTipFactory.FromPower<WeakPower>((int?)null),
		RawHoverTipBuilder.Build("danjin-stance-atk", "攻势1", "打出时需要至少[blue]1[/blue]层[red]攻势[/red]，否则该效果不生效。"),
		RawHoverTipBuilder.Build("danjin-stance-def", "守势1", "打出时需要至少[blue]1[/blue]层[blue]守势[/blue]，否则该效果不生效。")
	});

	protected override IEnumerable<DynamicVar> CanonicalVars => new _003C_003Ez__ReadOnlyArray<DynamicVar>((DynamicVar[])(object)new DynamicVar[4]
	{
		(DynamicVar)new DamageVar(8m, (ValueProp)8),
		new FeiRenVar(),
		new DynamicVar("StrengthDown", 3m),
		(DynamicVar)new PowerVar<WeakPower>(2m)
	});

	protected override int FeirenHitCount => 1;

	public PoJun()
		: base(1, (CardType)1, (CardRarity)3, (TargetType)2)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, "Target");
		Creature creature = ((CardModel)this).Owner.Creature;
		int gongShi = ((creature != null) ? creature.GetPowerAmount<GongShiPower>() : 0);
		Creature creature2 = ((CardModel)this).Owner.Creature;
		int shouShi = ((creature2 != null) ? creature2.GetPowerAmount<ShouShiPower>() : 0);
		await BeginFeirenAttack(choiceContext);
		decimal num = ScaleByFeiren(((DynamicVar)((CardModel)this).DynamicVars.Damage).BaseValue);
		NDimensionSlashVfx slashVfx = PrepareJianQiSlash(cardPlay.Target);
		slashVfx?.DoSlash();
		await DamageCmd.Attack(num).FromCard((CardModel)(object)this).Targeting(cardPlay.Target)
			.Execute(choiceContext);
		slashVfx?.ForceComplete();
		await StanceCmd.ReinforceStance(choiceContext, ((CardModel)this).Owner);
		if (gongShi >= 1 && cardPlay.Target.IsAlive)
		{
			int num2 = ScaleByFeiren((int)((CardModel)this).DynamicVars["StrengthDown"].BaseValue);
			if (num2 > 0)
			{
				await PowerCmd.Apply<PoJunStrengthDownPower>(choiceContext, cardPlay.Target, (decimal)num2, ((CardModel)this).Owner.Creature, (CardModel)(object)this, false);
			}
		}
		if (shouShi >= 1 && cardPlay.Target.IsAlive)
		{
			decimal num3 = ScaleByFeiren(((CardModel)this).DynamicVars["WeakPower"].BaseValue);
			if (num3 > 0m)
			{
				await PowerCmd.Apply<WeakPower>(choiceContext, cardPlay.Target, num3, ((CardModel)this).Owner.Creature, (CardModel)(object)this, false);
			}
		}
	}

	protected override void OnUpgrade()
	{
		((DynamicVar)((CardModel)this).DynamicVars.Damage).UpgradeValueBy(3m);
		((CardModel)this).DynamicVars["StrengthDown"].UpgradeValueBy(1m);
		((CardModel)this).DynamicVars["WeakPower"].UpgradeValueBy(1m);
	}
}
