using System.Threading.Tasks;
using Danjin.Cards;
using Danjin.Resources;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace Danjin.Powers;

public sealed class ChaNaFangHuaPower : DanjinPower
{
	public override PowerType Type => (PowerType)1;

	public override PowerStackType StackType => (PowerStackType)2;

	public override Task AfterCardChangedPiles(CardModel card, PileType oldPileType, AbstractModel? clonedBy)
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
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
		if (obj != ((PowerModel)this).Owner)
		{
			return Task.CompletedTask;
		}
		if (!card.Keywords.Contains(DanjinCardKeywords.FeiRen))
		{
			return Task.CompletedTask;
		}
		if (!card.Keywords.Contains((CardKeyword)2))
		{
			card.AddKeyword((CardKeyword)2);
		}
		return Task.CompletedTask;
	}

	public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
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
		if (obj == ((PowerModel)this).Owner && cardPlay.Card.Keywords.Contains(DanjinCardKeywords.FeiRen))
		{
			((PowerModel)this).Flash();
			await TonghuaCmd.GainTonghua(1m, ((PowerModel)this).Owner.Player);
		}
	}
}
