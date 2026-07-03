using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Danjin.Extensions;
using Danjin.Powers;
using Danjin.Resources;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Danjin.Cards;

public class WangChaoJianJi : DanjinCard
{
	public override IEnumerable<CardKeyword> CanonicalKeywords => new _003C_003Ez__ReadOnlySingleElementList<CardKeyword>(DanjinCardKeywords.CanXin);

	public override int CanonicalStarCost => 1;

	protected override HashSet<CardTag> CanonicalTags => new HashSet<CardTag> { (CardTag)0 };

	protected override IEnumerable<IHoverTip> AdditionalHoverTips => new _003C_003Ez__ReadOnlyArray<IHoverTip>((IHoverTip[])(object)new IHoverTip[4]
	{
		RawHoverTipBuilder.Build("danjin-stance-atk", "攻势1", "打出时需要至少[blue]1[/blue]层[red]攻势[/red]，否则该效果不生效。"),
		RawHoverTipBuilder.Build("danjin-stance-def", "守势1", "打出时需要至少[blue]1[/blue]层[blue]守势[/blue]，否则该效果不生效。"),
		HoverTipFactory.FromPower<ShiPoPower>((int?)null),
		HoverTipFactory.FromPower<ChuXuePower>((int?)null)
	});

	protected override IEnumerable<DynamicVar> CanonicalVars => new _003C_003Ez__ReadOnlySingleElementList<DynamicVar>((DynamicVar)new DamageVar(6m, (ValueProp)8));

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
				obj = ((creature != null) ? new int?(creature.GetPowerAmount<GongShiPower>()) : ((int?)null));
			}
			int? num = obj;
			if (num.GetValueOrDefault() <= 0)
			{
				Player owner2 = ((CardModel)this).Owner;
				int? obj2;
				if (owner2 == null)
				{
					obj2 = null;
				}
				else
				{
					Creature creature2 = owner2.Creature;
					obj2 = ((creature2 != null) ? new int?(creature2.GetPowerAmount<ShouShiPower>()) : ((int?)null));
				}
				num = obj2;
				return num.GetValueOrDefault() > 0;
			}
			return true;
		}
	}

	public WangChaoJianJi()
		: base(1, (CardType)1, (CardRarity)4, (TargetType)2)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, "Target");
		Creature creature = ((CardModel)this).Owner.Creature;
		int gongShi = ((creature != null) ? creature.GetPowerAmount<GongShiPower>() : 0);
		Creature creature2 = ((CardModel)this).Owner.Creature;
		int shouShi = ((creature2 != null) ? creature2.GetPowerAmount<ShouShiPower>() : 0);
		await StanceCmd.ReinforceStance(choiceContext, ((CardModel)this).Owner);
		AttackCommand val = await PlayJianQiAttackOnce(choiceContext, cardPlay.Target);
		if (gongShi >= 1)
		{
			int? obj;
			if (val == null)
			{
				obj = null;
			}
			else
			{
				DamageResult? obj2 = val.Results.SelectMany((List<DamageResult> r) => r).FirstOrDefault();
				obj = ((obj2 != null) ? new int?(obj2.TotalDamage) : ((int?)null));
			}
			int num = obj ?? ((int)((DynamicVar)((CardModel)this).DynamicVars.Damage).BaseValue);
			if (num > 0 && cardPlay.Target.IsAlive)
			{
				await PowerCmd.Apply<ChuXuePower>(choiceContext, cardPlay.Target, (decimal)num, ((CardModel)this).Owner.Creature, (CardModel)(object)this, false);
			}
		}
		else if (shouShi >= 1)
		{
			await PowerCmd.Apply<ShiPoPower>(choiceContext, ((CardModel)this).Owner.Creature, 1m, ((CardModel)this).Owner.Creature, (CardModel)(object)this, false);
		}
	}

	protected override void OnUpgrade()
	{
		((DynamicVar)((CardModel)this).DynamicVars.Damage).UpgradeValueBy(2m);
	}
}
