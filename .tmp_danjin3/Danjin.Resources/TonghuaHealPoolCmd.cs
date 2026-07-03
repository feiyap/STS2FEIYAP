using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Danjin.Powers;
using Danjin.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace Danjin.Resources;

public static class TonghuaHealPoolCmd
{
	private static readonly ConditionalWeakTable<PlayerCombatState, TonghuaHealPool> _pools = new ConditionalWeakTable<PlayerCombatState, TonghuaHealPool>();

	public static TonghuaHealPool GetPool(Player player)
	{
		if (player == null)
		{
			throw new ArgumentNullException("player");
		}
		return _pools.GetValue(player.PlayerCombatState, (PlayerCombatState _) => new TonghuaHealPool());
	}

	public static TonghuaHealPool GetPool(PlayerCombatState pcs)
	{
		if (pcs == null)
		{
			throw new ArgumentNullException("pcs");
		}
		return _pools.GetValue(pcs, (PlayerCombatState _) => new TonghuaHealPool());
	}

	public static void Add(int amount, Player player)
	{
		if (player != null && amount > 0)
		{
			GetPool(player).Add(amount);
		}
	}

	public static void Subtract(int amount, Player player)
	{
		if (player != null && amount > 0)
		{
			GetPool(player).Subtract(amount);
		}
	}

	public static int TryConsumeForHeal(int desired, Player player)
	{
		if (player == null)
		{
			return 0;
		}
		if (desired <= 0)
		{
			return 0;
		}
		return GetPool(player).TryConsumeForHeal(desired);
	}

	public static int PeekRemaining(Player player)
	{
		if (player == null)
		{
			return 0;
		}
		return GetPool(player).Remaining;
	}

	public static async Task SyncPower(PlayerChoiceContext choiceContext, Player player)
	{
		try
		{
			if (player == null || CombatManager.Instance == null || CombatManager.Instance.IsEnding)
			{
				return;
			}
			Creature creature = player.Creature;
			if (creature != null && creature.IsAlive)
			{
				int remaining = GetPool(player).Remaining;
				int powerAmount = creature.GetPowerAmount<TonghuaHealPoolPower>();
				int num = remaining - powerAmount;
				if (num != 0)
				{
					await PowerCmd.Apply<TonghuaHealPoolPower>(choiceContext, creature, (decimal)num, creature, (CardModel)null, true);
				}
			}
		}
		catch (Exception value)
		{
			DanjinLog.Verbose($">>>[DanjinMod] 彤血 power 同步失败: {value}");
		}
	}
}
