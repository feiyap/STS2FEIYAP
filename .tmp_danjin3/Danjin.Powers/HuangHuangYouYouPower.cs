using System.Collections.Generic;
using System.Threading.Tasks;
using Danjin.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace Danjin.Powers;

public sealed class HuangHuangYouYouPower : DanjinPower
{
	private readonly HashSet<CardModel> _playedHuanghuangsThisTurn = new HashSet<CardModel>();

	private readonly Dictionary<CardModel, int> _markedCards = new Dictionary<CardModel, int>();

	public override PowerType Type => (PowerType)1;

	public override PowerStackType StackType => (PowerStackType)2;

	public bool TryConsumeFirstPlay(CardModel huanghuangCard)
	{
		if (huanghuangCard == null)
		{
			return false;
		}
		return _playedHuanghuangsThisTurn.Add(huanghuangCard);
	}

	public void MarkCard(CardModel card)
	{
		if (card != null)
		{
			_markedCards.TryGetValue(card, out var value);
			_markedCards[card] = value + 1;
			DanjinLog.Verbose($">>>[DanjinMod] 晃晃悠悠(追踪)：标记 [{card.Title}] → 当前计数 {_markedCards[card]}");
		}
	}

	public int GetMarkCount(CardModel? card)
	{
		if (card == null)
		{
			return 0;
		}
		if (!_markedCards.TryGetValue(card, out var value))
		{
			return 0;
		}
		return value;
	}

	public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
	{
		if (player == ((PowerModel)this).Owner.Player)
		{
			_playedHuanghuangsThisTurn.Clear();
			await Task.CompletedTask;
		}
	}

	public override async Task BeforeCardPlayed(CardPlay cardPlay)
	{
		if (cardPlay.Card == null)
		{
			return;
		}
		Player owner = cardPlay.Card.Owner;
		if (((owner != null) ? owner.Creature : null) == ((PowerModel)this).Owner && cardPlay.IsFirstInSeries && _markedCards.TryGetValue(cardPlay.Card, out var value) && value > 0)
		{
			((PowerModel)this).Flash();
			DanjinLog.Verbose($">>>[DanjinMod] 晃晃悠悠(追踪)：[{cardPlay.Card.Title}] 打出前，触发额外抽 {value} 张牌");
			Player player = ((PowerModel)this).Owner.Player;
			if (player != null)
			{
				await CardPileCmd.Draw((PlayerChoiceContext)new ThrowingPlayerChoiceContext(), (decimal)value, player, false);
			}
		}
	}
}
