using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Danjin.Audio;
using Danjin.Patches;
using Danjin.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.ValueProps;

namespace Danjin.Cards;

public class FeiHongZhanFang : DanjinCard
{
	private static ModSound? _voice;

	public override IEnumerable<CardKeyword> CanonicalKeywords => Array.Empty<CardKeyword>();

	public override int CanonicalStarCost => 8;

	protected override HashSet<CardTag> CanonicalTags => new HashSet<CardTag> { (CardTag)0 };

	protected override IEnumerable<IHoverTip> AdditionalHoverTips => new _003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.FromCard<FeiShaBaoFa>(false));

	protected override IEnumerable<DynamicVar> CanonicalVars => new _003C_003Ez__ReadOnlyArray<DynamicVar>((DynamicVar[])(object)new DynamicVar[2]
	{
		(DynamicVar)new DamageVar(4m, (ValueProp)8),
		new DynamicVar("HitCount", 8m)
	});

	public FeiHongZhanFang()
		: base(2, (CardType)1, (CardRarity)4, (TargetType)3)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		DanjinVoice.Play(_voice ?? (_voice = new ModSound("res://Danjin/Sounds/Cards/fei_hong_zhan_fang.ogg")));
		int num = (int)((CardModel)this).DynamicVars["HitCount"].BaseValue;
		decimal baseValue = ((DynamicVar)((CardModel)this).DynamicVars.Damage).BaseValue;
		AoeJianQiSlashGroup slashVfx = PrepareAoeJianQiSlashRandom(num);
		await DamageCmd.Attack(baseValue).WithHitCount(num).FromCard((CardModel)(object)this)
			.TargetingAllOpponents(((CardModel)this).CombatState)
			.BeforeDamage(DanjinCard.DoSlashHook(slashVfx))
			.Execute(choiceContext);
		slashVfx?.ForceComplete();
		FeiShaBaoFa feiShaBaoFa = ((CardModel)this).CombatState.CreateCard<FeiShaBaoFa>(((CardModel)this).Owner);
		LianDuanPatch.RecentlyGenerated.Add((CardModel)(object)feiShaBaoFa);
		CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardToCombat((CardModel)(object)feiShaBaoFa, (PileType)2, ((CardModel)this).Owner, (CardPilePosition)1), 1.2f, (CardPreviewStyle)1);
	}

	protected override void OnUpgrade()
	{
		((CardModel)this).EnergyCost.UpgradeBy(-1);
	}
}
