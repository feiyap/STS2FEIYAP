using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Danjin.Extensions;
using Danjin.Powers;
using Danjin.Resources;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace Danjin.Cards;

public class NvZiFangShenShu : DanjinCard
{
	public override IEnumerable<CardKeyword> CanonicalKeywords => new _003C_003Ez__ReadOnlySingleElementList<CardKeyword>(DanjinCardKeywords.CanXin);

	protected override HashSet<CardTag> CanonicalTags => new HashSet<CardTag> { (CardTag)0 };

	protected override IEnumerable<IHoverTip> AdditionalHoverTips => new _003C_003Ez__ReadOnlyArray<IHoverTip>((IHoverTip[])(object)new IHoverTip[2]
	{
		HoverTipFactory.FromPower<WeakPower>((int?)null),
		RawHoverTipBuilder.Build("danjin-stance-def", "守势1", "打出时需要至少[blue]1[/blue]层[blue]守势[/blue]，否则该效果不生效。")
	});

	protected override IEnumerable<DynamicVar> CanonicalVars => new _003C_003Ez__ReadOnlyArray<DynamicVar>((DynamicVar[])(object)new DynamicVar[3]
	{
		(DynamicVar)new DamageVar(6m, (ValueProp)8),
		(DynamicVar)new PowerVar<WeakPower>(1m),
		(DynamicVar)new BlockVar(5m, (ValueProp)8)
	});

	protected override bool ShouldGlowGoldInternal
	{
		get
		{
			Player owner = ((CardModel)this).Owner;
			int? obj;
			if (owner == null)
			{
				obj = null;
			}
			else
			{
				Creature creature = owner.Creature;
				obj = ((creature != null) ? new int?(creature.GetPowerAmount<ShouShiPower>()) : ((int?)null));
			}
			int? num = obj;
			return num.GetValueOrDefault() >= 1;
		}
	}

	public NvZiFangShenShu()
		: base(1, (CardType)1, (CardRarity)2, (TargetType)2)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
		Creature creature = ((CardModel)this).Owner.Creature;
		int shouShi = ((creature != null) ? creature.GetPowerAmount<ShouShiPower>() : 0);
		await StanceCmd.ReinforceStance(choiceContext, ((CardModel)this).Owner);
		await PlayJianQiAttackOnce(choiceContext, cardPlay.Target);
		if (cardPlay.Target.IsAlive)
		{
			await PowerCmd.Apply<WeakPower>(choiceContext, cardPlay.Target, ((CardModel)this).DynamicVars["WeakPower"].BaseValue, ((CardModel)this).Owner.Creature, (CardModel)(object)this, false);
		}
		if (shouShi >= 1)
		{
			await CreatureCmd.GainBlock(((CardModel)this).Owner.Creature, ((CardModel)this).DynamicVars.Block, cardPlay, false);
		}
	}

	protected override void OnUpgrade()
	{
		((DynamicVar)((CardModel)this).DynamicVars.Damage).UpgradeValueBy(1m);
		((CardModel)this).DynamicVars["WeakPower"].UpgradeValueBy(1m);
	}
}
