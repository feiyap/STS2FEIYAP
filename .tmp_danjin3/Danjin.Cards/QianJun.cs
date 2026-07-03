using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Danjin.Powers;
using Danjin.Resources;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Danjin.Cards;

public class QianJun : DanjinCard
{
	protected override HashSet<CardTag> CanonicalTags => new HashSet<CardTag> { (CardTag)0 };

	protected override bool IsPlayable
	{
		get
		{
			Creature creature = ((CardModel)this).Owner.Creature;
			return creature == null || !creature.HasPower<KuangLuanPower>();
		}
	}

	protected override IEnumerable<IHoverTip> AdditionalHoverTips => new _003C_003Ez__ReadOnlyArray<IHoverTip>((IHoverTip[])(object)new IHoverTip[2]
	{
		HoverTipFactory.FromPower<WeakPower>((int?)null),
		HoverTipFactory.FromPower<ShouShiPower>((int?)null)
	});

	protected override IEnumerable<DynamicVar> CanonicalVars => Array.Empty<DynamicVar>();

	public QianJun()
		: base(1, (CardType)2, (CardRarity)3, (TargetType)1)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await PowerCmd.Apply<WeakPower>(choiceContext, ((CardModel)this).Owner.Creature, 1m, ((CardModel)this).Owner.Creature, (CardModel)(object)this, false);
		Creature creature = ((CardModel)this).Owner.Creature;
		WeakPower val = ((creature != null) ? creature.GetPower<WeakPower>() : null);
		if (val != null)
		{
			((PowerModel)val).SkipNextDurationTick = false;
		}
		await StanceCmd.ClearStanceFreeze(choiceContext, ((CardModel)this).Owner);
		await StanceCmd.SwitchToStance(choiceContext, ((CardModel)this).Owner, toAttack: false);
		await PowerCmd.Apply<QianJunPower>(choiceContext, ((CardModel)this).Owner.Creature, 1m, ((CardModel)this).Owner.Creature, (CardModel)(object)this, false);
	}

	protected override void OnUpgrade()
	{
		((CardModel)this).EnergyCost.UpgradeBy(-1);
	}
}
