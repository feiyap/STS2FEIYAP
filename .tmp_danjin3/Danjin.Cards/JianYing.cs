using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Danjin.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Danjin.Cards;

public class JianYing : DanjinTokenCard
{
	public override IEnumerable<CardKeyword> CanonicalKeywords => new _003C_003Ez__ReadOnlySingleElementList<CardKeyword>((CardKeyword)1);

	protected override HashSet<CardTag> CanonicalTags => new HashSet<CardTag> { (CardTag)0 };

	protected override IEnumerable<IHoverTip> AdditionalHoverTips => new _003C_003Ez__ReadOnlyArray<IHoverTip>((IHoverTip[])(object)new IHoverTip[2]
	{
		HoverTipFactory.FromPower<ChuXuePower>((int?)null),
		HoverTipFactory.FromPower<StrengthPower>((int?)null)
	});

	protected override IEnumerable<DynamicVar> CanonicalVars => new _003C_003Ez__ReadOnlyArray<DynamicVar>((DynamicVar[])(object)new DynamicVar[2]
	{
		(DynamicVar)new PowerVar<ChuXuePower>(3m),
		(DynamicVar)new PowerVar<StrengthPower>(3m)
	});

	public JianYing()
		: base(0, (CardType)2, (TargetType)2)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, "Target");
		if (cardPlay.Target.IsAlive)
		{
			int num = (int)((CardModel)this).DynamicVars["ChuXuePower"].BaseValue;
			await PowerCmd.Apply<ChuXuePower>(choiceContext, cardPlay.Target, (decimal)num, ((CardModel)this).Owner.Creature, (CardModel)(object)this, false);
			int loseAmount = (int)((CardModel)this).DynamicVars["StrengthPower"].BaseValue;
			if (loseAmount > 0 && cardPlay.Target.IsAlive)
			{
				await PowerCmd.Apply<StrengthPower>(choiceContext, cardPlay.Target, (decimal)(-loseAmount), ((CardModel)this).Owner.Creature, (CardModel)(object)this, false);
				await PowerCmd.Apply<JianYingPower>(choiceContext, cardPlay.Target, (decimal)loseAmount, ((CardModel)this).Owner.Creature, (CardModel)(object)this, false);
			}
		}
	}

	protected override void OnUpgrade()
	{
		((CardModel)this).DynamicVars["ChuXuePower"].UpgradeValueBy(1m);
		((CardModel)this).DynamicVars["StrengthPower"].UpgradeValueBy(1m);
	}
}
