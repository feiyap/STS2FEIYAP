using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Danjin.Audio;
using Danjin.Extensions;
using Danjin.Powers;
using Danjin.Resources;
using Danjin.Utils;
using Danjin.Variables;
using Danjin.Vfx;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Danjin.Cards;

public class XunJi : DanjinCard
{
	private static ModSound? _voice;

	public override IEnumerable<CardKeyword> CanonicalKeywords => new _003C_003Ez__ReadOnlyArray<CardKeyword>((CardKeyword[])(object)new CardKeyword[2]
	{
		(CardKeyword)(int)DanjinCardKeywords.FeiRen,
		(CardKeyword)(int)DanjinCardKeywords.CanXin
	});

	protected override HashSet<CardTag> CanonicalTags => new HashSet<CardTag> { (CardTag)0 };

	protected override IEnumerable<IHoverTip> AdditionalHoverTips => new _003C_003Ez__ReadOnlyArray<IHoverTip>((IHoverTip[])(object)new IHoverTip[2]
	{
		RawHoverTipBuilder.Build("danjin-stance-atk", "攻势1", "打出时需要至少[blue]1[/blue]层[red]攻势[/red]，否则该效果不生效。"),
		RawHoverTipBuilder.Build("danjin-stance-def", "守势1", "打出时需要至少[blue]1[/blue]层[blue]守势[/blue]，否则该效果不生效。")
	});

	protected override IEnumerable<DynamicVar> CanonicalVars => new _003C_003Ez__ReadOnlyArray<DynamicVar>((DynamicVar[])(object)new DynamicVar[3]
	{
		(DynamicVar)new DamageVar(6m, (ValueProp)8),
		new FeiRenVar(),
		(DynamicVar)new CardsVar(4)
	});

	protected override int FeirenHitCount => 1;

	public XunJi()
		: base(1, (CardType)1, (CardRarity)3, (TargetType)2)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		DanjinVoice.Play(_voice ?? (_voice = new ModSound("res://Danjin/Sounds/Cards/xun_ji.ogg")));
		ArgumentNullException.ThrowIfNull(cardPlay.Target, "Target");
		Creature creature = ((CardModel)this).Owner.Creature;
		int gongShi = ((creature != null) ? creature.GetPowerAmount<GongShiPower>() : 0);
		Creature creature2 = ((CardModel)this).Owner.Creature;
		int shouShi = ((creature2 != null) ? creature2.GetPowerAmount<ShouShiPower>() : 0);
		await BeginFeirenAttack(choiceContext);
		decimal num = ScaleByFeiren(((DynamicVar)((CardModel)this).DynamicVars.Damage).BaseValue);
		NDimensionSlashVfx slashVfx = PrepareJianQiSlash(cardPlay.Target);
		slashVfx?.DoSlash();
		await DamageCmd.Attack(num).FromCard((CardModel)(object)this).Targeting(cardPlay.Target)
			.Execute(choiceContext);
		slashVfx?.ForceComplete();
		await StanceCmd.ReinforceStance(choiceContext, ((CardModel)this).Owner);
		CardPile handPile = PileTypeExtensions.GetPile((PileType)2, ((CardModel)this).Owner);
		HashSet<CardModel> before = ((handPile != null) ? new HashSet<CardModel>(handPile.Cards) : new HashSet<CardModel>());
		int num2 = ScaleByFeiren((int)((DynamicVar)((CardModel)this).DynamicVars.Cards).BaseValue);
		if (num2 > 0)
		{
			await CardPileCmd.Draw(choiceContext, (decimal)num2, ((CardModel)this).Owner, false);
		}
		List<CardModel> source = ((handPile != null) ? handPile.Cards.Where((CardModel c) => c != null && !before.Contains(c)).ToList() : new List<CardModel>());
		List<CardModel> list = new List<CardModel>();
		if (gongShi >= 1)
		{
			list = source.Where((CardModel c) => (int)c.Type == 2 && c.DynamicVars.ContainsKey("Block")).ToList();
		}
		else if (shouShi >= 1)
		{
			list = source.Where((CardModel c) => (int)c.Type == 1).ToList();
		}
		foreach (CardModel item in list)
		{
			await CardCmd.Discard(choiceContext, item);
		}
	}

	protected override void OnUpgrade()
	{
		((DynamicVar)((CardModel)this).DynamicVars.Damage).UpgradeValueBy(3m);
		((DynamicVar)((CardModel)this).DynamicVars.Cards).UpgradeValueBy(1m);
	}
}
