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
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.ValueProps;

namespace Danjin.Cards;

public class JinMieIi : DanjinTokenCard
{
	public override IEnumerable<CardKeyword> CanonicalKeywords => new _003C_003Ez__ReadOnlyArray<CardKeyword>((CardKeyword[])(object)new CardKeyword[2]
	{
		(CardKeyword)(int)DanjinCardKeywords.FeiRen,
		(CardKeyword)(int)DanjinCardKeywords.LianDuan
	});

	protected override HashSet<CardTag> CanonicalTags => new HashSet<CardTag> { (CardTag)0 };

	protected override IEnumerable<DynamicVar> CanonicalVars => new _003C_003Ez__ReadOnlyArray<DynamicVar>((DynamicVar[])(object)new DynamicVar[4]
	{
		(DynamicVar)new DamageVar(4m, (ValueProp)8),
		new DynamicVar("HitCount", 2m),
		new FeiRenVar(),
		(DynamicVar)new PowerVar<StrengthPower>(1m)
	});

	protected override int FeirenHitCount => (int)((CardModel)this).DynamicVars["HitCount"].BaseValue;

	protected override IEnumerable<IHoverTip> AdditionalHoverTips
	{
		get
		{
			yield return HoverTipFactory.FromPower<StrengthPower>((int?)null);
			yield return RawHoverTipBuilder.Build("danjin-stance-atk", "攻势2", "打出时需要至少[blue]2[/blue]层[red]攻势[/red]，否则该效果不生效。");
			yield return HoverTipFactory.FromCard<JinMieIii>(((CardModel)this).IsUpgraded);
		}
	}

	public JinMieIi()
		: base(1, (CardType)1, (TargetType)2)
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
		if (gongShi >= 2)
		{
			decimal num3 = ScaleByFeiren(((CardModel)this).DynamicVars["StrengthPower"].BaseValue);
			if (num3 > 0m)
			{
				await PowerCmd.Apply<StrengthPower>(choiceContext, ((CardModel)this).Owner.Creature, num3, ((CardModel)this).Owner.Creature, (CardModel)(object)this, false);
			}
		}
		JinMieIii jinMieIii = ((CardModel)this).CombatState.CreateCard<JinMieIii>(((CardModel)this).Owner);
		if (((CardModel)this).IsUpgraded)
		{
			((CardModel)jinMieIii).UpgradeInternal();
			((CardModel)jinMieIii).FinalizeUpgradeInternal();
		}
		LianDuanPatch.RecentlyGenerated.Add((CardModel)(object)jinMieIii);
		CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardToCombat((CardModel)(object)jinMieIii, (PileType)2, ((CardModel)this).Owner, (CardPilePosition)1), 1.2f, (CardPreviewStyle)1);
	}

	protected override void OnUpgrade()
	{
		((DynamicVar)((CardModel)this).DynamicVars.Damage).UpgradeValueBy(1m);
	}
}
