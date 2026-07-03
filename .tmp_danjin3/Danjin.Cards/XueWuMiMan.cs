using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Danjin.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace Danjin.Cards;

public class XueWuMiMan : DanjinCard
{
	public override IEnumerable<CardKeyword> CanonicalKeywords => Array.Empty<CardKeyword>();

	protected override HashSet<CardTag> CanonicalTags => new HashSet<CardTag> { (CardTag)0 };

	protected override IEnumerable<IHoverTip> AdditionalHoverTips => new _003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.FromPower<ChuXuePower>((int?)null));

	protected override IEnumerable<DynamicVar> CanonicalVars => new _003C_003Ez__ReadOnlySingleElementList<DynamicVar>((DynamicVar)(object)new PowerVar<XueWuMiManPower>(2m));

	protected override bool ShouldGlowGoldInternal
	{
		get
		{
			Player owner = ((CardModel)this).Owner;
			bool? obj;
			if (owner == null)
			{
				obj = null;
			}
			else
			{
				Creature creature = owner.Creature;
				obj = ((creature != null) ? new bool?(creature.HasPower<XueWuMiManPower>()) : ((bool?)null));
			}
			bool? flag = obj;
			return flag == true;
		}
	}

	public XueWuMiMan()
		: base(1, (CardType)2, (CardRarity)4, (TargetType)1)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await PowerCmd.Apply<XueWuMiManPower>(choiceContext, ((CardModel)this).Owner.Creature, ((CardModel)this).DynamicVars["XueWuMiManPower"].BaseValue, ((CardModel)this).Owner.Creature, (CardModel)(object)this, false);
	}

	protected override void OnUpgrade()
	{
		((CardModel)this).DynamicVars["XueWuMiManPower"].UpgradeValueBy(1m);
	}
}
