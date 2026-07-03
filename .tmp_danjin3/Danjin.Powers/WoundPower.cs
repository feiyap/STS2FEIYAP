using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Danjin.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace Danjin.Powers;

public sealed class WoundPower : DanjinPower
{
	private const int MaxStrengthPenalty = 5;

	private const int WoundDecayInterval = 3;

	private const int WoundDecayAmount = 1;

	private int _appliedStrengthPenalty;

	private int _turnsSinceDecay;

	private bool _syncingStrength;

	private bool _resyncPending;

	private bool _readyForTickThisBatch;

	public override PowerType Type => (PowerType)2;

	public override PowerStackType StackType => (PowerStackType)1;

	private bool IsLiveOnOwner()
	{
		Creature owner = ((PowerModel)this).Owner;
		if (owner == null || !owner.IsAlive)
		{
			return false;
		}
		return owner.GetPower<WoundPower>() == this;
	}

	public override async Task AfterApplied(Creature? applier, CardModel? cardSource)
	{
		await base.AfterApplied(applier, cardSource);
		await SyncStrengthPenalty(applier, cardSource);
	}

	public override async Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
	{
		if ((object)power == this)
		{
			if (((PowerModel)this).Amount > 5)
			{
				await PowerCmd.ModifyAmount(choiceContext, (PowerModel)(object)this, (decimal)(5 - ((PowerModel)this).Amount), applier, cardSource, true);
			}
			await SyncStrengthPenalty(applier, cardSource);
		}
	}

	public override async Task AfterRemoved(Creature oldOwner)
	{
		if (_appliedStrengthPenalty > 0)
		{
			if (oldOwner == null)
			{
				_appliedStrengthPenalty = 0;
				return;
			}
			if (!oldOwner.IsAlive)
			{
				_appliedStrengthPenalty = 0;
				return;
			}
			if (!CombatManager.Instance.IsInProgress)
			{
				_appliedStrengthPenalty = 0;
				return;
			}
			int appliedStrengthPenalty = _appliedStrengthPenalty;
			_appliedStrengthPenalty = 0;
			DanjinLog.Verbose($">>>[DanjinMod] 伤口：被移除，还原 {appliedStrengthPenalty} 点力量");
			await PowerCmd.Apply<StrengthPower>((PlayerChoiceContext)new ThrowingPlayerChoiceContext(), oldOwner, (decimal)appliedStrengthPenalty, oldOwner, (CardModel)null, true);
		}
	}

	public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		if (((PowerModel)this).Owner != null && side == ((PowerModel)this).Owner.Side)
		{
			await TriggerWoundOnlyTick();
		}
	}

	public override Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
	{
		return Task.CompletedTask;
	}

	public override Task BeforeDamageReceived(PlayerChoiceContext choiceContext, Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		if (((PowerModel)this).Owner == null)
		{
			return Task.CompletedTask;
		}
		if (dealer != ((PowerModel)this).Owner)
		{
			return Task.CompletedTask;
		}
		if (!ValuePropExtensions.IsPoweredAttack(props))
		{
			return Task.CompletedTask;
		}
		_readyForTickThisBatch = true;
		return Task.CompletedTask;
	}

	public override async Task AfterDamageGiven(PlayerChoiceContext choiceContext, Creature? dealer, DamageResult result, ValueProp props, Creature target, CardModel? cardSource)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		if (((PowerModel)this).Owner != null && dealer == ((PowerModel)this).Owner && ValuePropExtensions.IsPoweredAttack(props) && _readyForTickThisBatch)
		{
			_readyForTickThisBatch = false;
			await TriggerWoundOnlyTick();
		}
	}

	private async Task TriggerWoundOnlyTick()
	{
		if (IsLiveOnOwner() && ((PowerModel)this).Amount > 0)
		{
			int num = 2 * ((PowerModel)this).Amount;
			if (num > 0)
			{
				((PowerModel)this).Flash();
				await CreatureCmd.Damage((PlayerChoiceContext)new ThrowingPlayerChoiceContext(), ((PowerModel)this).Owner, (decimal)num, (ValueProp)6, (Creature)null, (CardModel)null);
			}
		}
	}

	private async Task SyncStrengthPenalty(Creature? applier, CardModel? cardSource)
	{
		if (_syncingStrength)
		{
			_resyncPending = true;
			return;
		}
		_syncingStrength = true;
		try
		{
			do
			{
				_resyncPending = false;
				if (!CombatManager.Instance.IsInProgress || !IsLiveOnOwner())
				{
					break;
				}
				int target = Math.Min(((PowerModel)this).Amount, 5);
				if (target < 0)
				{
					target = 0;
				}
				int delta = target - _appliedStrengthPenalty;
				if (delta != 0)
				{
					int from = _appliedStrengthPenalty;
					_appliedStrengthPenalty = target;
					await PowerCmd.Apply<StrengthPower>((PlayerChoiceContext)new ThrowingPlayerChoiceContext(), ((PowerModel)this).Owner, (decimal)(-delta), applier, cardSource, true);
					DanjinLog.Verbose($">>>[DanjinMod] 伤口：层数 {((PowerModel)this).Amount}，同步力量扣减 {from}→{target}(本次力量 {((delta > 0) ? "-" : "+")}{Math.Abs(delta)})");
				}
			}
			while (_resyncPending);
		}
		finally
		{
			_syncingStrength = false;
		}
	}
}
