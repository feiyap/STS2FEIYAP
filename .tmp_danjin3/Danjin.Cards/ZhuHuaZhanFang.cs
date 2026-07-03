using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Danjin.Extensions;
using Danjin.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Danjin.Cards;

public class ZhuHuaZhanFang : DanjinCard
{
	public override IEnumerable<CardKeyword> CanonicalKeywords => new _003C_003Ez__ReadOnlySingleElementList<CardKeyword>((CardKeyword)1);

	protected override IEnumerable<IHoverTip> AdditionalHoverTips => new _003C_003Ez__ReadOnlyArray<IHoverTip>((IHoverTip[])(object)new IHoverTip[2]
	{
		HoverTipFactory.FromPower<ZhuShiZhiKePower>((int?)null),
		RawHoverTipBuilder.Build("danjin-stance-atk", "攻势2", "打出时需要至少[blue]2[/blue]层[red]攻势[/red]，否则该效果不生效。")
	});

	protected override HashSet<CardTag> CanonicalTags => new HashSet<CardTag> { (CardTag)0 };

	protected override IEnumerable<DynamicVar> CanonicalVars => new _003C_003Ez__ReadOnlyArray<DynamicVar>((DynamicVar[])(object)new DynamicVar[2]
	{
		(DynamicVar)new DamageVar(9m, (ValueProp)8),
		new DynamicVar("OtherEnemyStacks", 1m)
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
				obj = ((creature != null) ? new int?(creature.GetPowerAmount<GongShiPower>()) : ((int?)null));
			}
			int? num = obj;
			return num.GetValueOrDefault() >= 2;
		}
	}

	public ZhuHuaZhanFang()
		: base(1, (CardType)1, (CardRarity)2, (TargetType)2)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, "Target");
		Creature creature = ((CardModel)this).Owner.Creature;
		int gongShi = ((creature != null) ? creature.GetPowerAmount<GongShiPower>() : 0);
		await PlayJianQiAttackOnce(choiceContext, cardPlay.Target);
		if (cardPlay.Target.IsAlive)
		{
			ZhuShiZhiKePower power = cardPlay.Target.GetPower<ZhuShiZhiKePower>();
			int num = ((power != null) ? ((PowerModel)power).Amount : 0);
			if (num > 0)
			{
				await PowerCmd.Apply<ZhuShiZhiKePower>(choiceContext, cardPlay.Target, (decimal)num, ((CardModel)this).Owner.Creature, (CardModel)(object)this, false);
			}
		}
		if (gongShi < 2)
		{
			return;
		}
		decimal otherStacks = ((CardModel)this).DynamicVars["OtherEnemyStacks"].BaseValue;
		foreach (Creature hittableEnemy in ((CardModel)this).CombatState.HittableEnemies)
		{
			if (hittableEnemy != cardPlay.Target && hittableEnemy.IsAlive)
			{
				await PowerCmd.Apply<ZhuShiZhiKePower>(choiceContext, hittableEnemy, otherStacks, ((CardModel)this).Owner.Creature, (CardModel)(object)this, false);
			}
		}
	}

	protected override void OnUpgrade()
	{
		((DynamicVar)((CardModel)this).DynamicVars.Damage).UpgradeValueBy(3m);
	}
}
