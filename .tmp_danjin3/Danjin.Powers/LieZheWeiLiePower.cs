using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Danjin.Resources;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace Danjin.Powers;

public sealed class LieZheWeiLiePower : DanjinPower
{
	private const int AttackStanceGain = 4;

	private bool _restrictingThisTurn = true;

	private bool _buffActive;

	private readonly List<CardModel> _restricted = new List<CardModel>();

	public override PowerType Type => (PowerType)1;

	public override PowerStackType StackType => (PowerStackType)2;

	public void RestrictIfAttack(CardModel? card)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Invalid comparison between Unknown and I4
		if (_restrictingThisTurn && card != null && (int)card.Type == 1 && !card.Keywords.Contains((CardKeyword)4))
		{
			card.AddKeyword((CardKeyword)4);
			_restricted.Add(card);
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
			RestrictIfAttack(card);
		}
		return Task.CompletedTask;
	}

	public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
	{
		if (player.Creature != ((PowerModel)this).Owner || !_restrictingThisTurn)
		{
			return;
		}
		foreach (CardModel item in _restricted)
		{
			if (item.Keywords.Contains((CardKeyword)4))
			{
				item.RemoveKeyword((CardKeyword)4);
			}
		}
		_restricted.Clear();
		_restrictingThisTurn = false;
		await StanceCmd.GainAttackStance(choiceContext, ((PowerModel)this).Owner.Player, 4);
		_buffActive = true;
	}

	public override int ModifyCardPlayCount(CardModel card, Creature? target, int playCount)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Invalid comparison between Unknown and I4
		if (!_buffActive)
		{
			return playCount;
		}
		Player owner = card.Owner;
		if (((owner != null) ? owner.Creature : null) != ((PowerModel)this).Owner)
		{
			return playCount;
		}
		if ((int)card.Type != 1)
		{
			return playCount;
		}
		if (CombatManager.Instance.History.CardPlaysStarted.Count(delegate(CardPlayStartedEntry e)
		{
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0040: Invalid comparison between Unknown and I4
			if (((CombatHistoryEntry)e).Actor == ((PowerModel)this).Owner && e.CardPlay.IsFirstInSeries && ((CombatHistoryEntry)e).HappenedThisTurn(((PowerModel)this).CombatState))
			{
				CardModel card2 = e.CardPlay.Card;
				if (card2 == null)
				{
					return false;
				}
				return (int)card2.Type == 1;
			}
			return false;
		}) < 1)
		{
			return playCount + 1;
		}
		return playCount;
	}

	public override Task AfterModifyingCardPlayCount(CardModel card)
	{
		((PowerModel)this).Flash();
		return Task.CompletedTask;
	}

	public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		if (!_buffActive)
		{
			return;
		}
		CardModel card = cardPlay.Card;
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
			CardModel card2 = cardPlay.Card;
			if (card2 != null && (int)card2.Type == 1)
			{
				_buffActive = false;
				await PowerCmd.Remove((PowerModel)(object)this);
			}
		}
	}

	public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		if (side == ((PowerModel)this).Owner.Side && _buffActive)
		{
			await PowerCmd.Remove((PowerModel)(object)this);
		}
	}
}
