using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Danjin.Resources;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace Danjin.Cards;

public class XinDengYueYing : DanjinCard
{
	public override IEnumerable<CardKeyword> CanonicalKeywords => new _003C_003Ez__ReadOnlyArray<CardKeyword>((CardKeyword[])(object)new CardKeyword[2]
	{
		(CardKeyword)(int)DanjinCardKeywords.LiuZhuan,
		(CardKeyword)1
	});

	protected override HashSet<CardTag> CanonicalTags => new HashSet<CardTag> { (CardTag)0 };

	protected override IEnumerable<IHoverTip> AdditionalHoverTips => Array.Empty<IHoverTip>();

	protected override IEnumerable<DynamicVar> CanonicalVars => new _003C_003Ez__ReadOnlyArray<DynamicVar>((DynamicVar[])(object)new DynamicVar[3]
	{
		(DynamicVar)new EnergyVar(1),
		(DynamicVar)new StarsVar(1),
		new DynamicVar("DrawAmount", 1m)
	});

	public XinDengYueYing()
		: base(0, (CardType)2, (CardRarity)4, (TargetType)1)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await StanceCmd.SwitchStance(choiceContext, ((CardModel)this).Owner);
		await PlayerCmd.GainEnergy((decimal)((DynamicVar)((CardModel)this).DynamicVars.Energy).IntValue, ((CardModel)this).Owner);
		await TonghuaCmd.GainTonghua((int)((DynamicVar)((CardModel)this).DynamicVars.Stars).BaseValue, ((CardModel)this).Owner);
		await CardPileCmd.Draw(choiceContext, ((CardModel)this).DynamicVars["DrawAmount"].BaseValue, ((CardModel)this).Owner, false);
	}

	protected override void OnUpgrade()
	{
		((CardModel)this).DynamicVars["DrawAmount"].UpgradeValueBy(1m);
	}
}
