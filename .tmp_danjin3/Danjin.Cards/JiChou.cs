using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Danjin.Powers;
using Danjin.Variables;
using Danjin.Vfx;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Danjin.Cards;

public class JiChou : DanjinCard
{
	public override IEnumerable<CardKeyword> CanonicalKeywords => new _003C_003Ez__ReadOnlySingleElementList<CardKeyword>(DanjinCardKeywords.FeiRen);

	protected override HashSet<CardTag> CanonicalTags => new HashSet<CardTag> { (CardTag)0 };

	protected override IEnumerable<IHoverTip> AdditionalHoverTips => new _003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.FromPower<ZhuShiZhiKePower>((int?)null));

	protected override IEnumerable<DynamicVar> CanonicalVars => new _003C_003Ez__ReadOnlyArray<DynamicVar>((DynamicVar[])(object)new DynamicVar[3]
	{
		(DynamicVar)new DamageVar(9m, (ValueProp)8),
		new FeiRenVar(),
		new DynamicVar("JiChouTurns", 2m)
	});

	protected override int FeirenHitCount => 1;

	public JiChou()
		: base(1, (CardType)1, (CardRarity)3, (TargetType)2)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, "Target");
		await BeginFeirenAttack(choiceContext);
		decimal num = ScaleByFeiren(((DynamicVar)((CardModel)this).DynamicVars.Damage).BaseValue);
		NDimensionSlashVfx slashVfx = PrepareJianQiSlash(cardPlay.Target);
		slashVfx?.DoSlash();
		await DamageCmd.Attack(num).FromCard((CardModel)(object)this).Targeting(cardPlay.Target)
			.Execute(choiceContext);
		slashVfx?.ForceComplete();
		if (cardPlay.Target.IsAlive)
		{
			int num2 = ScaleByFeiren((int)((CardModel)this).DynamicVars["JiChouTurns"].BaseValue);
			if (num2 > 0)
			{
				await PowerCmd.Apply<JiChouPower>(choiceContext, cardPlay.Target, (decimal)num2, ((CardModel)this).Owner.Creature, (CardModel)(object)this, false);
			}
		}
	}

	protected override void OnUpgrade()
	{
		((DynamicVar)((CardModel)this).DynamicVars.Damage).UpgradeValueBy(3m);
		((CardModel)this).DynamicVars["JiChouTurns"].UpgradeValueBy(1m);
	}
}
