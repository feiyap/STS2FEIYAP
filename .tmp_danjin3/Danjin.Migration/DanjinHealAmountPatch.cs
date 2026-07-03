using System;
using Danjin.Hooks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;

namespace Danjin.Migration;

[HarmonyPatch(typeof(CreatureCmd), "Heal")]
internal static class DanjinHealAmountPatch
{
	[HarmonyPrefix]
	public static void Prefix(Creature creature, ref decimal amount)
	{
		if (creature == null || amount <= 0m)
		{
			return;
		}
		ICombatState combatState = creature.CombatState;
		if (combatState == null)
		{
			return;
		}
		try
		{
			decimal num = amount;
			decimal num2 = num;
			foreach (AbstractModel item in combatState.IterateHookListeners())
			{
				if (item is IDanjinHealAmountModifier danjinHealAmountModifier)
				{
					num2 += danjinHealAmountModifier.ModifyHealAdditive(creature, num);
				}
			}
			decimal val = num2;
			foreach (AbstractModel item2 in combatState.IterateHookListeners())
			{
				if (item2 is IDanjinHealAmountModifier danjinHealAmountModifier2)
				{
					val *= danjinHealAmountModifier2.ModifyHealMultiplicative(creature, num2);
				}
			}
			amount = Math.Max(0m, val);
		}
		catch (Exception value)
		{
			Log.Error($"[DanjinMod] DanjinHealAmountPatch.Prefix 异常: {value}", 1);
		}
	}
}
