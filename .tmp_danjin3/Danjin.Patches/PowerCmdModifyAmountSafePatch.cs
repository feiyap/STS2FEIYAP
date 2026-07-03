using System;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;

namespace Danjin.Patches;

[HarmonyPatch(typeof(PowerCmd), "ModifyAmount")]
internal static class PowerCmdModifyAmountSafePatch
{
	[HarmonyPostfix]
	private static void Postfix(ref Task<int> __result, PowerModel power)
	{
		Task<int> originalTask = __result;
		__result = WrapAsync(originalTask, power);
	}

	private static async Task<int> WrapAsync(Task<int> originalTask, PowerModel power)
	{
		try
		{
			return await originalTask.ConfigureAwait(continueOnCapturedContext: false);
		}
		catch (ObjectDisposedException ex)
		{
			string value = ((object)power)?.GetType().Name ?? "?";
			object obj;
			if (power == null)
			{
				obj = null;
			}
			else
			{
				Creature owner = power.Owner;
				obj = ((owner != null) ? owner.Name : null);
			}
			if (obj == null)
			{
				obj = "?";
			}
			string value2 = (string)obj;
			Log.Warn(">>>[DanjinMod] PowerCmd.ModifyAmount 视觉更新失败(AtlasTexture 已释放)，但 Power 数值已成功施加。" + $"Power={value}, Owner={value2}. 详细: {ex.Message}", 2);
			return (power != null) ? power.Amount : 0;
		}
	}
}
