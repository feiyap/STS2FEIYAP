using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Danjin.Extensions;
using Danjin.Powers;
using Danjin.Variables;
using Danjin.Vfx;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace Danjin.Cards;

public class QingSuan : DanjinCard
{
	public override IEnumerable<CardKeyword> CanonicalKeywords => new _003C_003Ez__ReadOnlySingleElementList<CardKeyword>((CardKeyword)5);

	protected override HashSet<CardTag> CanonicalTags => new HashSet<CardTag> { (CardTag)0 };

	protected override IEnumerable<IHoverTip> AdditionalHoverTips => new _003C_003Ez__ReadOnlySingleElementList<IHoverTip>(RawHoverTipBuilder.Build("danjin-stance-def", "守势4", "打出时需要至少[blue]4[/blue]层[blue]守势[/blue]，否则该效果不生效。"));

	protected override IEnumerable<DynamicVar> CanonicalVars => new _003C_003Ez__ReadOnlySingleElementList<DynamicVar>((DynamicVar)(object)new QingSuanDamageVar(1m));

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
			return num.GetValueOrDefault() >= 4;
		}
	}

	public QingSuan()
		: base(2, (CardType)1, (CardRarity)4, (TargetType)2)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, "Target");
		if (cardPlay.Target.IsAlive)
		{
			decimal baseValue = ((DynamicVar)((CardModel)this).DynamicVars.Damage).BaseValue;
			Creature creature = ((CardModel)this).Owner.Creature;
			if (((creature != null) ? creature.GetPowerAmount<ShouShiPower>() : 0) >= 4)
			{
				baseValue *= 2m;
			}
			NDimensionSlashVfx slashVfx = PrepareJianQiSlash(cardPlay.Target);
			await DamageCmd.Attack(baseValue).FromCard((CardModel)(object)this).Targeting(cardPlay.Target)
				.BeforeDamage(DanjinCard.DoSlashHook(slashVfx))
				.Execute(choiceContext);
		}
	}

	protected override void OnUpgrade()
	{
		((CardModel)this).EnergyCost.UpgradeBy(-1);
	}
}
