using System;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace Danjin.Powers;

public sealed class QingJiaoKaiShiPower : DanjinPower
{
	public override PowerType Type => (PowerType)1;

	public override PowerStackType StackType => (PowerStackType)1;

	public override bool TryModifyEnergyCostInCombat(CardModel card, decimal originalCost, out decimal modifiedCost)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Invalid comparison between Unknown and I4
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Invalid comparison between Unknown and I4
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Invalid comparison between Unknown and I4
		modifiedCost = originalCost;
		if (card.Owner.Creature != ((PowerModel)this).Owner)
		{
			return false;
		}
		if ((int)card.Type != 1)
		{
			return false;
		}
		CardPile pile = card.Pile;
		PileType? val = ((pile != null) ? new PileType?(pile.Type) : ((PileType?)null));
		if (!val.HasValue)
		{
			goto IL_006a;
		}
		PileType valueOrDefault = val.GetValueOrDefault();
		bool flag;
		if ((int)valueOrDefault != 2)
		{
			if ((int)valueOrDefault != 5)
			{
				goto IL_006a;
			}
			flag = true;
		}
		else
		{
			flag = true;
		}
		goto IL_006c;
		IL_006c:
		if (!flag)
		{
			return false;
		}
		modifiedCost = Math.Max(0m, originalCost - 1m);
		return true;
		IL_006a:
		flag = false;
		goto IL_006c;
	}

	public override async Task BeforeCardPlayed(CardPlay cardPlay)
	{
		if (cardPlay.Card.Owner.Creature != ((PowerModel)this).Owner || (int)cardPlay.Card.Type != 1)
		{
			return;
		}
		CardPile pile = cardPlay.Card.Pile;
		PileType? val = ((pile != null) ? new PileType?(pile.Type) : ((PileType?)null));
		if (!val.HasValue)
		{
			goto IL_009f;
		}
		PileType valueOrDefault = val.GetValueOrDefault();
		bool flag;
		if ((int)valueOrDefault != 2)
		{
			if ((int)valueOrDefault != 5)
			{
				goto IL_009f;
			}
			flag = true;
		}
		else
		{
			flag = true;
		}
		goto IL_00a1;
		IL_009f:
		flag = false;
		goto IL_00a1;
		IL_00a1:
		if (flag)
		{
			await PowerCmd.Apply<QingJiaoKaiShiPower>((PlayerChoiceContext)new BlockingPlayerChoiceContext(), ((PowerModel)this).Owner, -1m, ((PowerModel)this).Owner, (CardModel)null, true);
		}
	}
}
