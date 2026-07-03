using System;
using System.Collections.Generic;
using Danjin.Cards;
using HarmonyLib;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;

namespace Danjin.Patches;

[HarmonyPatch(/*Could not decode attribute arguments.*/)]
internal static class ArchaicToothTranscendencePatch
{
	[HarmonyPostfix]
	private static void Postfix(ref Dictionary<ModelId, CardModel> __result)
	{
		try
		{
			if (__result != null)
			{
				JianQi jianQi = ModelDb.Card<JianQi>();
				ChiHua chiHua = ModelDb.Card<ChiHua>();
				if (jianQi != null && chiHua != null && !__result.ContainsKey(((AbstractModel)jianQi).Id))
				{
					__result[((AbstractModel)jianQi).Id] = (CardModel)(object)chiHua;
				}
			}
		}
		catch (Exception value)
		{
			Log.Error($"[DanjinMod] ArchaicToothTranscendencePatch.Postfix 异常: {value}", 1);
		}
	}
}
