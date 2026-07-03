using System;
using System.Threading.Tasks;
using Danjin.Resources;
using Danjin.Utils;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Logging;

namespace Danjin.Patches;

internal static class TonghuaBattleEndHealPatch
{
	[HarmonyPatch(typeof(Hook), "AfterCombatVictory")]
	private static class BattleEndHealPatch
	{
		[HarmonyPostfix]
		private static void Postfix(ICombatState combatState, ref Task __result)
		{
			Task original = __result ?? Task.CompletedTask;
			__result = HealAfter(original, combatState);
		}

		private static async Task HealAfter(Task original, ICombatState combatState)
		{
			await original;
			try
			{
				if (combatState == null)
				{
					return;
				}
				foreach (Player player in combatState.Players)
				{
					await TryHealPlayer(player);
				}
			}
			catch (Exception value)
			{
				Log.Error($"[Danjin] TonghuaBattleEndHealPatch 战斗结束回血异常: {value}", 2);
			}
		}
	}

	private static async Task TryHealPlayer(Player? player)
	{
		if (player == null)
		{
			return;
		}
		Creature creature = player.Creature;
		if (creature == null || !creature.IsAlive)
		{
			return;
		}
		int tonghua = player.GetTonghua();
		if (tonghua <= 0)
		{
			return;
		}
		int num = (int)Math.Ceiling((decimal)creature.MaxHp * 0.03m * (decimal)tonghua);
		if (num > 0)
		{
			int num2 = TonghuaHealPoolCmd.TryConsumeForHeal(num, player);
			if (num2 <= 0)
			{
				DanjinLog.Verbose($">>>[DanjinMod] 战斗结束彤华疗愈：剩余 {tonghua} 彤华想回 {num} 点，但血池剩余 0 → 实际回 0");
			}
			else
			{
				DanjinLog.Verbose($">>>[DanjinMod] 战斗结束彤华疗愈：消耗剩余 {tonghua} 彤华，想回 {num}，血池实际供给 {num2} 点HP");
				await CreatureCmd.Heal(creature, (decimal)num2, true);
			}
		}
	}
}
