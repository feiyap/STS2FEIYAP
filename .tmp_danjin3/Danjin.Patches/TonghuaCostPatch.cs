using System;
using System.Threading.Tasks;
using Danjin.Character;
using Danjin.Resources;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;

namespace Danjin.Patches;

internal static class TonghuaCostPatch
{
	[HarmonyPatch(typeof(CardModel), "GetStarCostWithModifiers")]
	private static class GetStarCostWithModifiersPatch
	{
		[HarmonyPostfix]
		private static void Postfix(CardModel __instance, ref int __result)
		{
			try
			{
				if (__instance.HasStarCostX && ShouldUseTonghua(__instance))
				{
					Player owner = __instance.Owner;
					if (((owner != null) ? owner.PlayerCombatState : null) != null)
					{
						int value = TonghuaCmd.GetState(owner.PlayerCombatState).Value;
						__result = value;
					}
				}
			}
			catch (Exception value2)
			{
				Log.Error($"[Danjin] TonghuaCostPatch.GetStarCostWithModifiers Postfix 异常: {value2}", 2);
			}
		}
	}

	[HarmonyPatch(typeof(PlayerCombatState), "HasEnoughResourcesFor")]
	private static class HasEnoughResourcesForPatch
	{
		[HarmonyPostfix]
		private static void Postfix(PlayerCombatState __instance, CardModel card, ref UnplayableReason reason, ref bool __result)
		{
			try
			{
				if (!ShouldUseTonghua(card))
				{
					return;
				}
				int num = Math.Max(0, card.GetStarCostWithModifiers());
				if (num > 0)
				{
					bool flag = TonghuaCmd.GetState(__instance).Value >= num;
					bool flag2 = __instance.Stars >= num;
					if (!flag2 && flag)
					{
						reason = (UnplayableReason)((uint)reason & 0xFFFFFFDFu);
					}
					else if (flag2 && !flag)
					{
						reason = (UnplayableReason)((uint)reason | 0x20u);
					}
					__result = (int)reason == 0;
				}
			}
			catch (Exception value)
			{
				Log.Error($"[Danjin] TonghuaCostPatch.HasEnoughResourcesFor 修正失败: {value}", 2);
			}
		}
	}

	[HarmonyPatch(typeof(CardModel), "SpendStars")]
	private static class SpendStarsPatch
	{
		[HarmonyPrefix]
		private static bool Prefix(CardModel __instance, int amount, ref Task __result)
		{
			try
			{
				if (!ShouldUseTonghua(__instance))
				{
					return true;
				}
				Player val = ((__instance != null) ? __instance.Owner : null);
				if (val == null)
				{
					return true;
				}
				__result = SpendTonghuaInsteadOfStars(__instance, amount, val);
				return false;
			}
			catch (Exception value)
			{
				Log.Error($"[Danjin] TonghuaCostPatch.SpendStars Prefix 异常: {value}", 2);
				return true;
			}
		}

		private static async Task SpendTonghuaInsteadOfStars(CardModel card, int amount, Player owner)
		{
			if (!card.IsDupe)
			{
				try
				{
					card.LastStarsSpent = amount;
				}
				catch (Exception value)
				{
					Log.Error($"[Danjin] 写 LastStarsSpent 失败(可忽略): {value}", 2);
				}
			}
			if (amount > 0)
			{
				await TonghuaCmd.LoseTonghua(amount, owner);
			}
		}
	}

	private static bool ShouldUseTonghua(CardModel card)
	{
		if (card == null)
		{
			return false;
		}
		return card.Pool is DanjinCardPool;
	}
}
