using System.Collections.Generic;
using System.Threading.Tasks;
using Danjin.Cards;
using Danjin.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace Danjin.Powers;

public sealed class GongMingBaoFaPower : DanjinPower
{
	private readonly List<CardModel> _modifiedCards = new List<CardModel>();

	public override PowerType Type => (PowerType)1;

	public override PowerStackType StackType => (PowerStackType)2;

	public void MarkIfEligible(CardModel? card)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Invalid comparison between Unknown and I4
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Expected I4, but got Unknown
		if (card != null && (int)card.Type == 1 && card.CurrentStarCost <= 0 && !card.Keywords.Contains(DanjinCardKeywords.FeiRen))
		{
			CardCmd.ApplyKeyword(card, (CardKeyword[])(object)new CardKeyword[1] { (CardKeyword)(int)DanjinCardKeywords.FeiRen });
			_modifiedCards.Add(card);
			DanjinLog.Verbose(">>>[DanjinMod] 共鸣爆发：[" + card.Title + "] 视作绯刃");
		}
	}

	public override Task AfterCardChangedPiles(CardModel card, PileType oldPileType, AbstractModel? clonedBy)
	{
		object obj;
		if (card == null)
		{
			obj = null;
		}
		else
		{
			Player owner = card.Owner;
			obj = ((owner != null) ? owner.Creature : null);
		}
		if (obj == ((PowerModel)this).Owner)
		{
			MarkIfEligible(card);
		}
		return Task.CompletedTask;
	}

	public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		if (side != ((PowerModel)this).Owner.Side)
		{
			return;
		}
		foreach (CardModel modifiedCard in _modifiedCards)
		{
			if (modifiedCard.Keywords.Contains(DanjinCardKeywords.FeiRen))
			{
				CardCmd.RemoveKeyword(modifiedCard, (CardKeyword[])(object)new CardKeyword[1] { (CardKeyword)(int)DanjinCardKeywords.FeiRen });
			}
		}
		_modifiedCards.Clear();
		await PowerCmd.Remove((PowerModel)(object)this);
	}
}
