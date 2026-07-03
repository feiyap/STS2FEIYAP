using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Danjin.Variables;
using Danjin.Vfx;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace Danjin.Cards;

public class ZhuShiIi : DanjinTokenCard
{
	public override IEnumerable<CardKeyword> CanonicalKeywords => new _003C_003Ez__ReadOnlyArray<CardKeyword>((CardKeyword[])(object)new CardKeyword[2]
	{
		(CardKeyword)(int)DanjinCardKeywords.FeiRen,
		(CardKeyword)(int)DanjinCardKeywords.LianDuan
	});

	protected override HashSet<CardTag> CanonicalTags => new HashSet<CardTag> { (CardTag)0 };

	protected override IEnumerable<DynamicVar> CanonicalVars => new _003C_003Ez__ReadOnlyArray<DynamicVar>((DynamicVar[])(object)new DynamicVar[3]
	{
		(DynamicVar)new ZhuShiIiDamageVar(5m),
		new FeiRenVar(),
		new DynamicVar("ZhuShiDamageBonus", 2m)
	});

	protected override int FeirenHitCount => 1;

	protected override IEnumerable<IHoverTip> AdditionalHoverTips => new _003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.FromPower<ZhuShiZhiKePower>((int?)null));

	public ZhuShiIi()
		: base(1, (CardType)1, (TargetType)2)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
		await BeginFeirenAttack(choiceContext);
		ZhuShiZhiKePower power = cardPlay.Target.GetPower<ZhuShiZhiKePower>();
		int num = ((power != null) ? ((PowerModel)power).Amount : 0);
		decimal baseValue = ((CardModel)this).DynamicVars["ZhuShiDamageBonus"].BaseValue;
		decimal value = ((DynamicVar)((CardModel)this).DynamicVars.Damage).BaseValue + (decimal)num * baseValue;
		decimal num2 = ScaleByFeiren(value);
		NDimensionSlashVfx slashVfx = PrepareJianQiSlash(cardPlay.Target);
		slashVfx?.DoSlash();
		await DamageCmd.Attack(num2).FromCard((CardModel)(object)this).Targeting(cardPlay.Target)
			.Execute(choiceContext);
		slashVfx?.ForceComplete();
	}

	protected override void OnUpgrade()
	{
		((CardModel)this).DynamicVars["ZhuShiDamageBonus"].UpgradeValueBy(1m);
	}
}
