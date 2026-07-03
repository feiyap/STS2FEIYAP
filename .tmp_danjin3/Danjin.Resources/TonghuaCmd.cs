using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Players;

namespace Danjin.Resources;

public static class TonghuaCmd
{
	private static readonly ConditionalWeakTable<PlayerCombatState, TonghuaState> _states = new ConditionalWeakTable<PlayerCombatState, TonghuaState>();

	public static event Func<int, Player, Task>? AfterTonghuaGained;

	public static event Func<int, Player, Task>? AfterTonghuaSpent;

	public static TonghuaState GetState(Player player)
	{
		if (player == null)
		{
			throw new ArgumentNullException("player");
		}
		return _states.GetValue(player.PlayerCombatState, (PlayerCombatState _) => new TonghuaState());
	}

	public static TonghuaState GetState(PlayerCombatState pcs)
	{
		if (pcs == null)
		{
			throw new ArgumentNullException("pcs");
		}
		return _states.GetValue(pcs, (PlayerCombatState _) => new TonghuaState());
	}

	public static async Task GainTonghua(decimal amount, Player player)
	{
		if (player == null)
		{
			throw new ArgumentNullException("player");
		}
		if (amount < 0m)
		{
			throw new ArgumentException("Must not be negative.", "amount");
		}
		if (!CombatManager.Instance.IsEnding)
		{
			TonghuaState state = GetState(player);
			int value = state.Value;
			state.GainTonghua(amount);
			int num = state.Value - value;
			if (num > 0)
			{
				await InvokeMulticast(TonghuaCmd.AfterTonghuaGained, num, player);
			}
		}
	}

	public static async Task LoseTonghua(decimal amount, Player player)
	{
		if (player == null)
		{
			throw new ArgumentNullException("player");
		}
		if (amount < 0m)
		{
			throw new ArgumentException("Must not be negative.", "amount");
		}
		if (!CombatManager.Instance.IsEnding)
		{
			TonghuaState state = GetState(player);
			int value = state.Value;
			state.LoseTonghua(amount);
			int num = value - state.Value;
			if (num > 0)
			{
				await InvokeMulticast(TonghuaCmd.AfterTonghuaSpent, num, player);
			}
		}
	}

	public static async Task SetTonghua(decimal amount, Player player)
	{
		if (player == null)
		{
			throw new ArgumentNullException("player");
		}
		if (amount < 0m)
		{
			throw new ArgumentException("Must not be negative.", "amount");
		}
		if (!CombatManager.Instance.IsEnding)
		{
			int value = GetState(player).Value;
			if ((decimal)value < amount)
			{
				await GainTonghua(amount - (decimal)value, player);
			}
			else if ((decimal)value > amount)
			{
				await LoseTonghua((decimal)value - amount, player);
			}
		}
	}

	private static async Task InvokeMulticast(Func<int, Player, Task>? handler, int amount, Player player)
	{
		if (handler != null)
		{
			Delegate[] invocationList = handler.GetInvocationList();
			for (int i = 0; i < invocationList.Length; i++)
			{
				await ((Func<int, Player, Task>)invocationList[i])(amount, player);
			}
		}
	}
}
