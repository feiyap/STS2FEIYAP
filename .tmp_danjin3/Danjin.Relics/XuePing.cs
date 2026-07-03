using System;
using System.Threading.Tasks;
using Danjin.Cards;
using Danjin.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.ValueProps;

namespace Danjin.Relics;

public sealed class XuePing : DanjinRelic
{
	private bool _consumed;

	private CardModel? _protectedCard;

	public bool Consumed
	{
		get
		{
			return _consumed;
		}
		private set
		{
			((AbstractModel)this).AssertMutable();
			_consumed = value;
		}
	}

	private CardModel? ProtectedCard
	{
		get
		{
			return _protectedCard;
		}
		set
		{
			((AbstractModel)this).AssertMutable();
			_protectedCard = value;
		}
	}

	public override RelicRarity Rarity => (RelicRarity)2;

	public override Task BeforeCombatStart()
	{
		Consumed = false;
		ProtectedCard = null;
		((RelicModel)this).Status = (RelicStatus)1;
		((RelicModel)this).Flash();
		return Task.CompletedTask;
	}

	public override Task AfterCombatEnd(CombatRoom _)
	{
		Consumed = false;
		ProtectedCard = null;
		((RelicModel)this).Status = (RelicStatus)0;
		return Task.CompletedTask;
	}

	public override Task BeforeCardPlayed(CardPlay cardPlay)
	{
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		if (Consumed)
		{
			return Task.CompletedTask;
		}
		if (((RelicModel)this).Owner != cardPlay.Card.Owner)
		{
			return Task.CompletedTask;
		}
		if (!CombatManager.Instance.IsInProgress)
		{
			return Task.CompletedTask;
		}
		CardModel card = cardPlay.Card;
		if (!card.Keywords.Contains(DanjinCardKeywords.FeiRen))
		{
			return Task.CompletedTask;
		}
		ProtectedCard = card;
		return Task.CompletedTask;
	}

	public override Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
	{
		if (Consumed)
		{
			return Task.CompletedTask;
		}
		if (ProtectedCard == null)
		{
			return Task.CompletedTask;
		}
		if (cardPlay.Card == ProtectedCard)
		{
			DanjinLog.Verbose(">>>[DanjinMod] 血瓶: 本场首张绯刃 [" + cardPlay.Card.Title + "] 的扣血已被豁免");
			Consumed = true;
			ProtectedCard = null;
			((RelicModel)this).Status = (RelicStatus)0;
			((RelicModel)this).Flash();
		}
		return Task.CompletedTask;
	}

	public override decimal ModifyDamageAdditive(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
	{
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		if (Consumed)
		{
			return 0m;
		}
		if (ProtectedCard == null)
		{
			return 0m;
		}
		if (cardSource == null || cardSource != ProtectedCard)
		{
			return 0m;
		}
		if (target != null)
		{
			Player owner = ((RelicModel)this).Owner;
			if (target == ((owner != null) ? owner.Creature : null))
			{
				if (!((Enum)props).HasFlag((Enum)(object)(ValueProp)8))
				{
					return 0m;
				}
				if (!((Enum)props).HasFlag((Enum)(object)(ValueProp)4))
				{
					return 0m;
				}
				return -amount;
			}
		}
		return 0m;
	}
}
