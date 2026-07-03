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

public class KuangLuan : DanjinCard
{
	protected override HashSet<CardTag> CanonicalTags => new HashSet<CardTag> { (CardTag)0 };

	protected override bool IsPlayable
	{
		get
		{
			Creature creature = ((CardModel)this).Owner.Creature;
			return creature == null || !creature.HasPower<QianJunPower>();
		}
	}

	protected override IEnumerable<IHoverTip> AdditionalHoverTips => new _003C_003Ez__ReadOnlyArray<IHoverTip>((IHoverTip[])(object)new IHoverTip[2]
	{
		HoverTipFactory.FromPower<VulnerablePower>((int?)null),
		HoverTipFactory.FromPower<GongShiPower>((int?)null)
	});

	protected override IEnumerable<DynamicVar> CanonicalVars => Array.Empty<DynamicVar>();

	public KuangLuan()
		: base(1, (CardType)2, (CardRarity)3, (TargetType)1)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await PowerCmd.Apply<VulnerablePower>(choiceContext, ((CardModel)this).Owner.Creature, 1m, ((CardModel)this).Owner.Creature, (CardModel)(object)this, false);
		Creature creature = ((CardModel)this).Owner.Creature;
		VulnerablePower val = ((creature != null) ? creature.GetPower<VulnerablePower>() : null);
		if (val != null)
		{
			((PowerModel)val).SkipNextDurationTick = false;
		}
		await StanceCmd.ClearStanceFreeze(choiceContext, ((CardModel)this).Owner);
		await StanceCmd.SwitchToStance(choiceContext, ((CardModel)this).Owner, toAttack: true);
		await PowerCmd.Apply<KuangLuanPower>(choiceContext, ((CardModel)this).Owner.Creature, 1m, ((CardModel)this).Owner.Creature, (CardModel)(object)this, false);
	}

	protected override void OnUpgrade()
	{
		((CardModel)this).EnergyCost.UpgradeBy(-1);
	}
}
