using System;
using System.Collections.Generic;
using Danjin.Relics;
using HarmonyLib;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;

namespace Danjin.Patches;

[HarmonyPatch(/*Could not decode attribute arguments.*/)]
internal static class TouchOfOrobasRefinementPatch
{
	[HarmonyPostfix]
	private static void Postfix(ref Dictionary<ModelId, RelicModel> __result)
	{
		try
		{
			if (__result != null)
			{
				HuaiJinZhiYu huaiJinZhiYu = ModelDb.Relic<HuaiJinZhiYu>();
				FeiYu feiYu = ModelDb.Relic<FeiYu>();
				if (huaiJinZhiYu != null && feiYu != null && !__result.ContainsKey(((AbstractModel)huaiJinZhiYu).Id))
				{
					__result[((AbstractModel)huaiJinZhiYu).Id] = (RelicModel)(object)feiYu;
				}
			}
		}
		catch (Exception value)
		{
			Log.Error($"[DanjinMod] TouchOfOrobasRefinementPatch.Postfix 异常: {value}", 1);
		}
	}
}
