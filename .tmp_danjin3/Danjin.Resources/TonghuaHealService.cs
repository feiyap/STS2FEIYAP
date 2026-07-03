using System;
using System.Threading.Tasks;
using Danjin.Powers;
using Danjin.Utils;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Danjin.Resources;

public static class TonghuaHealService
{
	[HarmonyPatch(typeof(ModelDb), "Init")]
	private static class InitPatch
	{
		[HarmonyPostfix]
		private static void Postfix()
		{
			EnsureSubscribed();
		}
	}

	public const decimal HealPercentPerStar = 0.03m;

	private static bool _subscribed;

	public static void EnsureSubscribed()
	{
		if (!_subscribed)
		{
			TonghuaCmd.AfterTonghuaSpent += OnTonghuaSpent;
			_subscribed = true;
		}
	}

	private static async Task OnTonghuaSpent(int amount, Player spender)
	{
		if (amount <= 0 || spender == null || CombatManager.Instance == null || !CombatManager.Instance.IsInProgress)
		{
			return;
		}
		Creature creature = spender.Creature;
		if (creature == null || !creature.IsAlive)
		{
			return;
		}
		if (creature.HasPower<DanXinZhaoMingYuePower>())
		{
			await CreatureCmd.GainBlock(creature, (decimal)amount, (ValueProp)4, (CardPlay)null, false);
		}
		int num = (int)Math.Ceiling((decimal)creature.MaxHp * 0.03m * (decimal)amount);
		if (num <= 0)
		{
			return;
		}
		int actualHeal = TonghuaHealPoolCmd.TryConsumeForHeal(num, spender);
		if (actualHeal <= 0)
		{
			DanjinLog.Verbose($">>>[DanjinMod] 彤华回血：消耗 {amount} 彤华想回 {num} 点，但血池剩余 0 → 实际回 0");
			return;
		}
		DanjinLog.Verbose($">>>[DanjinMod] 彤华回血：消耗 {amount} 彤华，想回 {num}，血池实际供给 {actualHeal} 点HP");
		await CreatureCmd.Heal(creature, (decimal)actualHeal, true);
		if (creature.HasPower<GuYanNanHongPower>())
		{
			int halfHeal = actualHeal / 2;
			if (halfHeal > 0)
			{
				ICombatState combatState = creature.CombatState;
				if (combatState != null)
				{
					foreach (Player player in combatState.Players)
					{
						Creature creature2 = player.Creature;
						if (creature2 != null && creature2 != creature && creature2.IsAlive)
						{
							await CreatureCmd.Heal(creature2, (decimal)halfHeal, true);
						}
					}
				}
			}
		}
		await TonghuaHealPoolCmd.SyncPower((PlayerChoiceContext)new BlockingPlayerChoiceContext(), spender);
	}
}
