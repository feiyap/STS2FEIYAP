using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Danjin.Character;
using Danjin.Powers;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;

namespace Danjin.Patches;

[HarmonyPatch]
[HarmonyPriority(600)]
internal class StanceDescDimPatch
{
	private const string DimOpen = "[color=#888888]";

	private const string DimClose = "[/color]";

	private static readonly Regex SwitchMarker = new Regex("\\[b\\]\\[(?:blue|red)\\]→(攻势|守势)\\[/(?:blue|red)\\]\\[/b\\][：:]");

	private static readonly Regex NewCondMarker = new Regex("\\[b\\]\\[(?:red|blue)\\](攻势|守势)(\\d*)\\[/(?:red|blue)\\]\\[/b\\][：:]");

	private static readonly Regex OldCondMarker = new Regex("(?<!→)\\[gold\\](攻势|守势)\\[/gold\\](\\d*)[：:]");

	private static readonly Regex StyleTag = new Regex("\\[/?(gold|blue|red|green|orange|pink|purple|teal|b|color[^\\]]*)\\]");

	private static MethodBase TargetMethod()
	{
		Type nestedType = typeof(CardModel).GetNestedType("DescriptionPreviewType", BindingFlags.NonPublic);
		if (nestedType == null)
		{
			throw new MissingMemberException(typeof(CardModel).FullName, "DescriptionPreviewType");
		}
		return AccessTools.Method(typeof(CardModel), "GetDescriptionForPile", new Type[3]
		{
			typeof(PileType),
			nestedType,
			typeof(Creature)
		}, (Type[])null) ?? throw new MissingMethodException(typeof(CardModel).FullName, "GetDescriptionForPile(PileType, DescriptionPreviewType, Creature)");
	}

	[HarmonyPostfix]
	private static void Postfix(CardModel __instance, PileType pileType, ref string __result)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Invalid comparison between Unknown and I4
		if ((int)pileType != 2 || !(__instance.Pool is DanjinCardPool) || (!__result.Contains("攻势") && !__result.Contains("守势")))
		{
			return;
		}
		int powerAmount;
		int powerAmount2;
		try
		{
			Player owner = __instance.Owner;
			Creature val = ((owner != null) ? owner.Creature : null);
			if (((val != null) ? val.CombatState : null) == null)
			{
				return;
			}
			powerAmount = val.GetPowerAmount<GongShiPower>();
			powerAmount2 = val.GetPowerAmount<ShouShiPower>();
		}
		catch
		{
			return;
		}
		List<(int, string, int, bool)> list = new List<(int, string, int, bool)>();
		foreach (Match item in SwitchMarker.Matches(__result))
		{
			list.Add((item.Index, item.Groups[1].Value, 0, true));
		}
		foreach (Match item2 in NewCondMarker.Matches(__result))
		{
			string value = item2.Groups[2].Value;
			list.Add((item2.Index, item2.Groups[1].Value, string.IsNullOrEmpty(value) ? 1 : int.Parse(value), false));
		}
		foreach (Match item3 in OldCondMarker.Matches(__result))
		{
			string value2 = item3.Groups[2].Value;
			list.Add((item3.Index, item3.Groups[1].Value, string.IsNullOrEmpty(value2) ? 1 : int.Parse(value2), false));
		}
		if (list.Count == 0)
		{
			return;
		}
		list = (from m in list
			group m by m.pos into g
			select g.First() into m
			orderby m.pos
			select m).ToList();
		for (int num = list.Count - 1; num >= 0; num--)
		{
			var (num2, text, num3, flag) = list[num];
			int num4;
			if (num < list.Count - 1)
			{
				num4 = list[num + 1].Item1;
			}
			else
			{
				int num5 = __result.IndexOf('。', num2);
				num4 = ((num5 >= 0) ? num5 : __result.Length);
			}
			int num6 = num2;
			if (!((!flag) ? ((text == "攻势") ? (powerAmount >= num3) : (powerAmount2 >= num3)) : ((text == "攻势") ? (powerAmount2 > 0) : (powerAmount > 0))))
			{
				string obj2 = __result;
				int num7 = num6;
				string input = obj2.Substring(num7, num4 - num7);
				input = StyleTag.Replace(input, "");
				input = "[color=#888888]" + input + "[/color]";
				__result = __result.Substring(0, num6) + input + __result.Substring(num4);
			}
		}
	}
}
