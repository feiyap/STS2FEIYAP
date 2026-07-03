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

namespace Danjin.Cards;

public class XiuLuoXingTai : DanjinCard
{
	public override IEnumerable<CardKeyword> CanonicalKeywords => Array.Empty<CardKeyword>();

	protected override HashSet<CardTag> CanonicalTags => new HashSet<CardTag> { (CardTag)0 };

	protected override IEnumerable<IHoverTip> AdditionalHoverTips => new _003C_003Ez__ReadOnlyArray<IHoverTip>((IHoverTip[])(object)new IHoverTip[2]
	{
		HoverTipFactory.FromPower<GongShiPower>((int?)null),
		HoverTipFactory.FromPower<ShouShiPower>((int?)null)
	});

	protected override IEnumerable<DynamicVar> CanonicalVars => new _003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new DynamicVar("CapBonus", 1m));

	public XiuLuoXingTai()
		: base(3, (CardType)3, (CardRarity)4, (TargetType)1)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		decimal baseValue = ((CardModel)this).DynamicVars["CapBonus"].BaseValue;
		await PowerCmd.Apply<XiuLuoXingTaiPower>(choiceContext, ((CardModel)this).Owner.Creature, baseValue, ((CardModel)this).Owner.Creature, (CardModel)(object)this, false);
		XiuLuoXingTaiPower power = ((CardModel)this).Owner.Creature.GetPower<XiuLuoXingTaiPower>();
		if (power != null)
		{
			await power.Resync(choiceContext);
		}
	}

	protected override void OnUpgrade()
	{
		((CardModel)this).DynamicVars["CapBonus"].UpgradeValueBy(1m);
	}
}
