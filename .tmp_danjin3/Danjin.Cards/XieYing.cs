using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Danjin.Resources;
using Danjin.Variables;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace Danjin.Cards;

public class XieYing : DanjinCard
{
	public override IEnumerable<CardKeyword> CanonicalKeywords => new _003C_003Ez__ReadOnlySingleElementList<CardKeyword>((CardKeyword)1);

	protected override HashSet<CardTag> CanonicalTags => new HashSet<CardTag> { (CardTag)0 };

	protected override IEnumerable<IHoverTip> AdditionalHoverTips => new _003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.FromPower<ZhuShiZhiKePower>((int?)null));

	protected override IEnumerable<DynamicVar> CanonicalVars => new _003C_003Ez__ReadOnlySingleElementList<DynamicVar>((DynamicVar)(object)new ZhuShiZhiKeVar());

	public XieYing()
		: base(1, (CardType)2, (CardRarity)3, (TargetType)2)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, "Target");
		if (!cardPlay.Target.IsAlive)
		{
			return;
		}
		decimal baseValue = ((CardModel)this).DynamicVars["ZhuShiZhiKeStacks"].BaseValue;
		if (baseValue > 0m)
		{
			await PowerCmd.Apply<ZhuShiZhiKePower>(choiceContext, cardPlay.Target, baseValue, ((CardModel)this).Owner.Creature, (CardModel)(object)this, false);
		}
		if (cardPlay.Target.IsAlive)
		{
			int powerAmount = cardPlay.Target.GetPowerAmount<ZhuShiZhiKePower>();
			if (powerAmount > 0)
			{
				await TonghuaCmd.GainTonghua(powerAmount, ((CardModel)this).Owner);
			}
		}
	}

	protected override void OnUpgrade()
	{
		((CardModel)this).DynamicVars["ZhuShiZhiKeStacks"].UpgradeValueBy(1m);
	}
}
