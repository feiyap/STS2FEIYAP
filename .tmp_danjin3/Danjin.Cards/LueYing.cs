using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Danjin.Audio;
using Danjin.Utils;
using Danjin.Vfx;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.ValueProps;

namespace Danjin.Cards;

public class LueYing : DanjinCard
{
	private static ModSound? _voice;

	public override IEnumerable<CardKeyword> CanonicalKeywords => Array.Empty<CardKeyword>();

	protected override HashSet<CardTag> CanonicalTags => new HashSet<CardTag> { (CardTag)0 };

	protected override IEnumerable<DynamicVar> CanonicalVars => new _003C_003Ez__ReadOnlySingleElementList<DynamicVar>((DynamicVar)new DamageVar(6m, (ValueProp)8));

	protected override IEnumerable<IHoverTip> AdditionalHoverTips
	{
		get
		{
			yield return HoverTipFactory.FromCard<HuanYing>(false);
		}
	}

	public LueYing()
		: base(1, (CardType)1, (CardRarity)2, (TargetType)2)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		DanjinVoice.Play(_voice ?? (_voice = new ModSound("res://Danjin/Sounds/Cards/lue_ying.ogg")));
		ArgumentNullException.ThrowIfNull(cardPlay.Target, "Target");
		decimal baseValue = ((DynamicVar)((CardModel)this).DynamicVars.Damage).BaseValue;
		NDimensionSlashVfx slashVfx = PrepareJianQiSlash(cardPlay.Target);
		slashVfx?.DoSlash();
		await DamageCmd.Attack(baseValue).FromCard((CardModel)(object)this).Targeting(cardPlay.Target)
			.Execute(choiceContext);
		slashVfx?.ForceComplete();
		CardPile pile = PileTypeExtensions.GetPile((PileType)2, ((CardModel)this).Owner);
		if (pile == null || pile.Cards.Count <= 0)
		{
			return;
		}
		CardSelectorPrefs val = new CardSelectorPrefs(((CardModel)this).SelectionScreenPrompt, 1);
		foreach (CardModel item in await CardSelectCmd.FromHand(choiceContext, ((CardModel)this).Owner, val, (Func<CardModel, bool>)null, (AbstractModel)(object)this))
		{
			HuanYing huanYing = ((CardModel)this).CombatState.CreateCard<HuanYing>(((CardModel)this).Owner);
			await CardCmd.Transform(item, (CardModel)(object)huanYing, (CardPreviewStyle)1);
		}
	}

	protected override void OnUpgrade()
	{
		((DynamicVar)((CardModel)this).DynamicVars.Damage).UpgradeValueBy(3m);
	}
}
