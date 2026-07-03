using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Danjin.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Danjin.Powers;

public sealed class ChuXuePower : DanjinPower
{
	internal const int FixedTickDamage = 1;

	private const int BaseBurstThreshold = 10;

	private const int MinBurstThreshold = 5;

	private const int BurstDamage = 10;

	private const int BleedDamagePerWound = 1;

	internal const int TickBaseWoundCap = 10;

	private const int BurstBaseWoundCap = 20;

	private const int WoundPerBurst = 1;

	private const int BurstStackCost = 10;

	private const int MaxBurstsPerInvocation = 64;

	private const int ForecastResetDelayMs = 600;

	private bool _readyForTickThisBatch;

	private bool _burstLoopRunning;

	private int _lastStableForecastTickDamage;

	private long _resetGeneration;

	internal bool IsProcessingTickOrBurst { get; private set; }

	public override PowerType Type => (PowerType)2;

	public override PowerStackType StackType => (PowerStackType)1;

	internal int GetForecastTickDamage()
	{
		if (IsProcessingTickOrBurst)
		{
			return _lastStableForecastTickDamage;
		}
		return _lastStableForecastTickDamage = ComputeBleedDamage(1, 10);
	}

	private void ScheduleForecastReset()
	{
		long myGen = Interlocked.Increment(ref _resetGeneration);
		ResetAfterDelayAsync(myGen);
	}

	private async Task ResetAfterDelayAsync(long myGen)
	{
		try
		{
			await Task.Delay(600);
		}
		catch
		{
			return;
		}
		if (Interlocked.Read(in _resetGeneration) == myGen)
		{
			IsProcessingTickOrBurst = false;
		}
	}

	private bool IsLiveOnOwner()
	{
		Creature owner = ((PowerModel)this).Owner;
		if (owner == null || !owner.IsAlive)
		{
			return false;
		}
		return owner.GetPower<ChuXuePower>() == this;
	}

	public override Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
	{
		return Task.CompletedTask;
	}

	public override Task BeforeDamageReceived(PlayerChoiceContext choiceContext, Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
	{
		return Task.CompletedTask;
	}

	public override Task AfterDamageGiven(PlayerChoiceContext choiceContext, Creature? dealer, DamageResult result, ValueProp props, Creature target, CardModel? cardSource)
	{
		return Task.CompletedTask;
	}

	public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		if (((PowerModel)this).Owner != null && side == ((PowerModel)this).Owner.Side && ((PowerModel)this).Amount > 0 && IsLiveOnOwner())
		{
			MonsterModel monster = ((PowerModel)this).Owner.Monster;
			if (monster != null && monster.IntendsToAttack)
			{
				DanjinLog.Verbose(">>>[DanjinMod] 出血：怪物意图为攻击 —— 本回合层数不衰减");
				return;
			}
			int num = ((PowerModel)this).Amount / 2;
			int num2 = ((PowerModel)this).Amount - num;
			DanjinLog.Verbose($">>>[DanjinMod] 出血：非攻击意图，层数 {((PowerModel)this).Amount} → {num}(-{num2})");
			await PowerCmd.ModifyAmount(choiceContext, (PowerModel)(object)this, -(decimal)num2, ((PowerModel)this).Owner, (CardModel)null, true);
		}
	}

	public override async Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
	{
		if ((object)power != this || amount <= 0m || _burstLoopRunning || !IsLiveOnOwner())
		{
			return;
		}
		_burstLoopRunning = true;
		try
		{
			int safety = 0;
			while (IsLiveOnOwner() && ((PowerModel)this).Amount >= GetEffectiveBurstThreshold())
			{
				int num = safety + 1;
				safety = num;
				if (num > 64)
				{
					Log.Warn(">>>[DanjinMod] 出血爆发：单次结算达到硬熔断上限 " + $"{64}，强制停止本轮(疑似异常状态，已防御)", 2);
					break;
				}
				await TriggerBurst(choiceContext, applier, cardSource);
				if (!IsLiveOnOwner())
				{
					break;
				}
			}
		}
		finally
		{
			_burstLoopRunning = false;
		}
	}

	private async Task TriggerTickDamage()
	{
		if (!IsLiveOnOwner() || ((PowerModel)this).Amount <= 0)
		{
			return;
		}
		int num = ComputeBleedDamage(1, 10);
		if (num <= 0)
		{
			return;
		}
		((PowerModel)this).Flash();
		_lastStableForecastTickDamage = num;
		IsProcessingTickOrBurst = true;
		try
		{
			await CreatureCmd.Damage((PlayerChoiceContext)new ThrowingPlayerChoiceContext(), ((PowerModel)this).Owner, (decimal)num, (ValueProp)6, (Creature)null, (CardModel)null);
		}
		finally
		{
			ScheduleForecastReset();
		}
	}

	private async Task TriggerBurst(PlayerChoiceContext choiceContext, Creature? applier, CardModel? cardSource)
	{
		if (!IsLiveOnOwner())
		{
			return;
		}
		Creature owner = ((PowerModel)this).Owner;
		int num = ComputeBleedDamage(10, 20);
		DanjinLog.Verbose($">>>[DanjinMod] 出血爆发：当前层数 {((PowerModel)this).Amount}，造成 {num} 点伤害(10 + {1}×Wound 封顶 {20}，再经咒血/血流不止加法叠加放大)，+{1} 伤口，层数 -{GetEffectiveBurstThreshold()}");
		((PowerModel)this).Flash();
		_lastStableForecastTickDamage = ComputeBleedDamage(1, 10);
		IsProcessingTickOrBurst = true;
		try
		{
			await CreatureCmd.Damage((PlayerChoiceContext)new ThrowingPlayerChoiceContext(), owner, (decimal)num, (ValueProp)6, (Creature)null, (CardModel)null);
			if (owner != null && owner.IsAlive)
			{
				int thresholdBeforeWound = GetEffectiveBurstThreshold();
				await PowerCmd.Apply<WoundPower>(choiceContext, owner, 1m, applier, cardSource, false);
				if (owner != null && owner.IsAlive && owner.GetPower<ChuXuePower>() == this)
				{
					await PowerCmd.ModifyAmount(choiceContext, (PowerModel)(object)this, -(decimal)thresholdBeforeWound, applier, cardSource, true);
				}
			}
		}
		finally
		{
			ScheduleForecastReset();
		}
	}

	internal static int ComputeBleedDamageFor(Creature owner, int baseDamage, int baseWoundCap)
	{
		if (owner == null)
		{
			return 0;
		}
		WoundPower power = owner.GetPower<WoundPower>();
		int num = ((power != null) ? ((PowerModel)power).Amount : 0);
		if (num < 0)
		{
			num = 0;
		}
		decimal num2 = (decimal)baseDamage + 1m * (decimal)num;
		if (baseWoundCap > 0 && num2 > (decimal)baseWoundCap)
		{
			num2 = baseWoundCap;
		}
		decimal d = num2;
		if (!owner.IsPlayer)
		{
			decimal num3 = 1m;
			if (AnyPlayerHasZhouXueFor(owner))
			{
				num3 += 0.5m;
			}
			if (num3 != 1m)
			{
				d *= num3;
			}
		}
		return (int)Math.Floor(d);
	}

	internal int ComputeBleedDamage(int baseDamage, int baseWoundCap)
	{
		return ComputeBleedDamageFor(((PowerModel)this).Owner, baseDamage, baseWoundCap);
	}

	private static bool AnyPlayerHasZhouXueFor(Creature owner)
	{
		ICombatState val = ((owner != null) ? owner.CombatState : null);
		if (val == null)
		{
			return false;
		}
		foreach (Player player in val.Players)
		{
			if (player.Creature != null && player.Creature.HasPower<ZhouXuePower>())
			{
				return true;
			}
		}
		return false;
	}

	private int GetEffectiveBurstThreshold()
	{
		if (((PowerModel)this).Owner == null || ((PowerModel)this).Owner.IsPlayer)
		{
			return 10;
		}
		ShangKouSiLiePower power = ((PowerModel)this).Owner.GetPower<ShangKouSiLiePower>();
		int num = ((power != null) ? ((PowerModel)power).Amount : 0);
		if (num <= 0)
		{
			return 10;
		}
		WoundPower power2 = ((PowerModel)this).Owner.GetPower<WoundPower>();
		int num2 = ((power2 != null) ? ((PowerModel)power2).Amount : 0);
		if (num2 <= 0)
		{
			return 10;
		}
		int num3 = 10 - num2 * num;
		if (num3 < 5)
		{
			num3 = 5;
		}
		return num3;
	}
}
