using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace Danjin.Cards;

public class SheShenJian : DanjinCard
{
	public override IEnumerable<CardKeyword> CanonicalKeywords => new _003C_003Ez__ReadOnlySingleElementList<CardKeyword>((CardKeyword)1);

	protected override HashSet<CardTag> CanonicalTags => new HashSet<CardTag> { (CardTag)0 };

	protected override IEnumerable<DynamicVar> CanonicalVars => new _003C_003Ez__ReadOnlyArray<DynamicVar>((DynamicVar[])(object)new DynamicVar[2]
	{
		(DynamicVar)new DamageVar(15m, (ValueProp)8),
		(DynamicVar)new PowerVar<WeakPower>(2m)
	});

	protected override IEnumerable<IHoverTip> AdditionalHoverTips => new _003C_003Ez__ReadOnlyArray<IHoverTip>((IHoverTip[])(object)new IHoverTip[3]
	{
		HoverTipFactory.FromPower<ArtifactPower>((int?)null),
		HoverTipFactory.FromPower<WeakPower>((int?)null),
		HoverTipFactory.Static((StaticHoverTip)5, Array.Empty<DynamicVar>())
	});

	public SheShenJian()
		: base(1, (CardType)1, (CardRarity)3, (TargetType)2)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, "Target");
		decimal weak = ((CardModel)this).DynamicVars["WeakPower"].BaseValue;
		await PlayJianQiAttackOnce(choiceContext, cardPlay.Target);
		if (cardPlay.Target.IsAlive)
		{
			if (cardPlay.Target.Block > 0)
			{
				await CreatureCmd.LoseBlock(cardPlay.Target, (decimal)cardPlay.Target.Block);
			}
			if (cardPlay.Target.HasPower<ArtifactPower>())
			{
				await PowerCmd.Remove<ArtifactPower>(cardPlay.Target);
			}
			await PowerCmd.Apply<WeakPower>(choiceContext, cardPlay.Target, weak, ((CardModel)this).Owner.Creature, (CardModel)(object)this, false);
		}
		await PowerCmd.Apply<WeakPower>(choiceContext, ((CardModel)this).Owner.Creature, weak, ((CardModel)this).Owner.Creature, (CardModel)(object)this, false);
		Creature creature = ((CardModel)this).Owner.Creature;
		WeakPower val = ((creature != null) ? creature.GetPower<WeakPower>() : null);
		if (val != null)
		{
			((PowerModel)val).SkipNextDurationTick = false;
		}
	}

	protected override void OnUpgrade()
	{
		((DynamicVar)((CardModel)this).DynamicVars.Damage).UpgradeValueBy(5m);
	}
}
