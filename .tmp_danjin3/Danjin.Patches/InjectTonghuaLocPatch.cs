using System.Collections.Generic;
using HarmonyLib;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;

namespace Danjin.Patches;

[HarmonyPatch(typeof(ModelDb), "Init")]
internal static class InjectTonghuaLocPatch
{
	[HarmonyPostfix]
	private static void Inject()
	{
		LocTable table = LocManager.Instance.GetTable("static_hover_tips");
		if (table != null && AccessTools.Field(typeof(LocTable), "_translations")?.GetValue(table) is Dictionary<string, string> dictionary)
		{
			dictionary["TONGHUA_COUNT.title"] = "彤华";
			dictionary["TONGHUA_COUNT.description"] = "丹瑾的核心资源。打出特定卡牌可以获得和消耗{singleTonghuaIcon}。战斗结束时，消耗剩余彤华并回复生命值。";
		}
	}
}
