using System.Collections.Generic;
using System.Threading.Tasks;
using Danjin.Cards;
using Danjin.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.CommonUi;

namespace Danjin.Powers;

public sealed class MingJingZhiShuiPower : DanjinPower
{
	public override PowerType Type => (PowerType)1;

	public override PowerStackType StackType => (PowerStackType)2;

	public async Task TransformExistingHandStatusCards()
	{
		Creature owner = ((PowerModel)this).Owner;
		if (((owner != null) ? owner.Player : null) == null)
		{
			return;
		}
		CardPile pile = PileTypeExtensions.GetPile((PileType)2, ((PowerModel)this).Owner.Player);
		if (pile == null || pile.Cards.Count == 0)
		{
			return;
		}
		List<CardModel> list = new List<CardModel>(pile.Cards);
		foreach (CardModel item in list)
		{
			await TryTransformStatusToMingXiang(item);
		}
	}

	public override async Task AfterCardChangedPiles(CardModel card, PileType oldPileType, AbstractModel? source)
	{
		await TryTransformStatusToMingXiang(card);
	}

	private async Task TryTransformStatusToMingXiang(CardModel card)
	{
		if (card != null && card.Owner == ((PowerModel)this).Owner.Player && (int)card.Type == 4 && card.Pile != null && (int)card.Pile.Type == 2)
		{
			if (!card.IsTransformable)
			{
				DanjinLog.Verbose(">>>[DanjinMod] 明镜止水：状态牌 " + ((AbstractModel)card).Id.Entry + " 不可被变化，跳过");
				return;
			}
			DanjinLog.Verbose(">>>[DanjinMod] 明镜止水：变化手牌中的状态牌 " + ((AbstractModel)card).Id.Entry + " 为冥想");
			((PowerModel)this).Flash();
			MingXiang mingXiang = ((PowerModel)this).Owner.CombatState.CreateCard<MingXiang>(((PowerModel)this).Owner.Player);
			await CardCmd.Transform(card, (CardModel)(object)mingXiang, (CardPreviewStyle)1);
		}
	}

	public void OnCardBeingTransformedAway(CardModel original)
	{
	}
}
