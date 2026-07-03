using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Danjin.Extensions;
using Danjin.Patches;
using Danjin.Powers;
using Danjin.Variables;
using Danjin.Vfx;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.ValueProps;

namespace Danjin.Cards;

public class ZhuShiI : DanjinCard
{
	public override IEnumerable<CardKeyword> CanonicalKeywords => new _003C_003Ez__ReadOnlySingleElementList<CardKeyword>(DanjinCardKeywords.FeiRen);

	protected override HashSet<CardTag> CanonicalTags => new HashSet<CardTag> { (CardTag)0 };

	protected override IEnumerable<DynamicVar> CanonicalVars => new _003C_003Ez__ReadOnlyArray<DynamicVar>((DynamicVar[])(object)new DynamicVar[4]
	{
		(DynamicVar)new DamageVar(4m, (ValueProp)8),
		new DynamicVar("HitCount", 2m),
		new FeiRenVar(),
		new ZhuShiZhiKeVar(2m)
	});

	protected override int FeirenHitCount => (int)((CardModel)this).DynamicVars["HitCount"].BaseValue;

	protected override IEnumerable<IHoverTip> AdditionalHoverTips
	{
		get
		{
			yield return HoverTipFactory.FromPower<ZhuShiZhiKePower>((int?)null);
			yield return HoverTipFactory.FromCard<ZhuShiIi>(((CardModel)this).IsUpgraded);
			yield return RawHoverTipBuilder.Build("danjin-stance-atk", "攻势1", "打出时需要至少[blue]1[/blue]层[red]攻势[/red]，否则该效果不生效。");
			yield return HoverTipFactory.FromKeyword(DanjinCardKeywords.LianDuan);
		}
	}

	public ZhuShiI()
		: base(1, (CardType)1, (CardRarity)3, (TargetType)2)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
		Creature creature = ((CardModel)this).Owner.Creature;
		int gongShi = ((creature != null) ? creature.GetPowerAmount<GongShiPower>() : 0);
		await BeginFeirenAttack(choiceContext);
		int num = (int)((CardModel)this).DynamicVars["HitCount"].BaseValue;
		decimal num2 = ScaleByFeiren(((DynamicVar)((CardModel)this).DynamicVars.Damage).BaseValue);
		NDimensionSlashVfx slashVfx = PrepareJianQiSlash(cardPlay.Target, num);
		if (cardPlay.Target.IsAlive)
		{
			await DamageCmd.Attack(num2).WithHitCount(num).FromCard((CardModel)(object)this)
				.Targeting(cardPlay.Target)
				.BeforeDamage(DanjinCard.DoSlashHook(slashVfx))
				.Execute(choiceContext);
		}
		slashVfx?.ForceComplete();
		if (cardPlay.Target.IsAlive)
		{
			decimal num3 = ScaleByFeiren(((CardModel)this).DynamicVars["ZhuShiZhiKeStacks"].BaseValue);
			if (num3 > 0m)
			{
				await PowerCmd.Apply<ZhuShiZhiKePower>(choiceContext, cardPlay.Target, num3, ((CardModel)this).Owner.Creature, (CardModel)(object)this, false);
			}
		}
		if (gongShi >= 1)
		{
			ZhuShiIi zhuShiIi = ((CardModel)this).CombatState.CreateCard<ZhuShiIi>(((CardModel)this).Owner);
			if (((CardModel)this).IsUpgraded)
			{
				((CardModel)zhuShiIi).UpgradeInternal();
				((CardModel)zhuShiIi).FinalizeUpgradeInternal();
			}
			LianDuanPatch.RecentlyGenerated.Add((CardModel)(object)zhuShiIi);
			CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardToCombat((CardModel)(object)zhuShiIi, (PileType)2, ((CardModel)this).Owner, (CardPilePosition)1), 1.2f, (CardPreviewStyle)1);
		}
	}

	protected override void OnUpgrade()
	{
		((DynamicVar)((CardModel)this).DynamicVars.Damage).UpgradeValueBy(1m);
		((CardModel)this).DynamicVars["ZhuShiZhiKeStacks"].UpgradeValueBy(1m);
	}
}
