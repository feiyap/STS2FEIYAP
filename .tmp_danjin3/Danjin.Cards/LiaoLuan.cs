using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Danjin.Patches;
using Danjin.Resources;
using Danjin.Variables;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.ValueProps;

namespace Danjin.Cards;

public class LiaoLuan : DanjinCard
{
	public override IEnumerable<CardKeyword> CanonicalKeywords
	{
		get
		{
			if (!((CardModel)this).IsUpgraded)
			{
				return Array.Empty<CardKeyword>();
			}
			return (IEnumerable<CardKeyword>)(object)new CardKeyword[1] { (CardKeyword)5 };
		}
	}

	public override bool HasStarCostX => true;

	public override int CanonicalStarCost => 0;

	protected override HashSet<CardTag> CanonicalTags => new HashSet<CardTag> { (CardTag)0 };

	protected override IEnumerable<DynamicVar> CanonicalVars => new _003C_003Ez__ReadOnlyArray<DynamicVar>((DynamicVar[])(object)new DynamicVar[3]
	{
		(DynamicVar)new DamageVar(4m, (ValueProp)8),
		new DynamicVar("BonusDamage", 2m),
		new LiaoYuVar()
	});

	protected override IEnumerable<IHoverTip> AdditionalHoverTips
	{
		get
		{
			yield return HoverTipFactory.FromCard<FenLuo>(false);
			yield return HoverTipFactory.FromKeyword(DanjinCardKeywords.LianDuan);
		}
	}

	protected override bool ShouldGlowGoldInternal
	{
		get
		{
			Player owner = ((CardModel)this).Owner;
			if (((owner != null) ? owner.PlayerCombatState : null) == null)
			{
				return false;
			}
			return ((CardModel)this).Owner.GetTonghua() >= 6;
		}
	}

	public LiaoLuan()
		: base(0, (CardType)1, (CardRarity)3, (TargetType)4)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		int num = ((CardModel)this).ResolveStarXValue();
		if (num > 0)
		{
			decimal baseValue = ((DynamicVar)((CardModel)this).DynamicVars.Damage).BaseValue;
			bool triggerBonus = num >= 6;
			decimal num2 = (triggerBonus ? ((CardModel)this).DynamicVars["BonusDamage"].BaseValue : 0m);
			decimal num3 = baseValue + num2;
			AoeJianQiSlashGroup slashVfx = PrepareAoeJianQiSlashRandom(num);
			await DamageCmd.Attack(num3).WithHitCount(num).FromCard((CardModel)(object)this)
				.TargetingRandomOpponents(((CardModel)this).CombatState, true)
				.BeforeDamage(DanjinCard.DoSlashHook(slashVfx))
				.Execute(choiceContext);
			if (triggerBonus)
			{
				FenLuo fenLuo = ((CardModel)this).CombatState.CreateCard<FenLuo>(((CardModel)this).Owner);
				LianDuanPatch.RecentlyGenerated.Add((CardModel)(object)fenLuo);
				CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardToCombat((CardModel)(object)fenLuo, (PileType)2, ((CardModel)this).Owner, (CardPilePosition)1), 1.2f, (CardPreviewStyle)1);
			}
			slashVfx?.ForceComplete();
		}
	}

	protected override void OnUpgrade()
	{
		((CardModel)this).AddKeyword((CardKeyword)5);
	}
}
