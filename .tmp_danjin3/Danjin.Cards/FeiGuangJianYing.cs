using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Danjin.Audio;
using Danjin.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.CommonUi;

namespace Danjin.Cards;

public class FeiGuangJianYing : DanjinCard
{
	private static ModSound? _voice;

	public override IEnumerable<CardKeyword> CanonicalKeywords => Array.Empty<CardKeyword>();

	protected override HashSet<CardTag> CanonicalTags => new HashSet<CardTag> { (CardTag)0 };

	protected override IEnumerable<IHoverTip> AdditionalHoverTips
	{
		get
		{
			yield return HoverTipFactory.FromCard<JianYing>(((CardModel)this).IsUpgraded);
		}
	}

	protected override IEnumerable<DynamicVar> CanonicalVars => Array.Empty<DynamicVar>();

	public FeiGuangJianYing()
		: base(2, (CardType)2, (CardRarity)4, (TargetType)1)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		DanjinVoice.Play(_voice ?? (_voice = new ModSound("res://Danjin/Sounds/Cards/fei_guang.ogg")));
		CardPile pile = PileTypeExtensions.GetPile((PileType)2, ((CardModel)this).Owner);
		if (pile == null || pile.Cards.Count == 0)
		{
			return;
		}
		int count = pile.Cards.Count;
		CardSelectorPrefs val = new CardSelectorPrefs(((CardModel)this).SelectionScreenPrompt, 0, count);
		((CardSelectorPrefs)(ref val)).set_Cancelable(false);
		val = val;
		foreach (CardModel item in await CardSelectCmd.FromHand(choiceContext, ((CardModel)this).Owner, val, (Func<CardModel, bool>)null, (AbstractModel)(object)this))
		{
			JianYing jianYing = ((CardModel)this).CombatState.CreateCard<JianYing>(((CardModel)this).Owner);
			if (((CardModel)this).IsUpgraded)
			{
				CardCmd.Upgrade((CardModel)(object)jianYing, (CardPreviewStyle)1);
			}
			await CardCmd.Transform(item, (CardModel)(object)jianYing, (CardPreviewStyle)1);
		}
	}

	protected override void OnUpgrade()
	{
	}
}
