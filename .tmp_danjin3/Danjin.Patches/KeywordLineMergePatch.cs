using System;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Danjin.Character;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;

namespace Danjin.Patches;

[HarmonyPatch]
[HarmonyAfter(new string[] { "com.ritsukage.sts2-RitsuLib.framework-content-assets" })]
[HarmonyPriority(200)]
internal class KeywordLineMergePatch
{
	private static readonly Regex BbcodeTag = new Regex("\\[/?[^\\]]+\\]");

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
	private static void Postfix(CardModel __instance, ref string __result)
	{
		MergeKeywordLines(__instance, ref __result);
	}

	internal static void MergeKeywordLines(CardModel card, ref string result)
	{
		if (!(card.Pool is DanjinCardPool) || !result.Contains('\n'))
		{
			return;
		}
		string[] array = result.Split('\n');
		bool flag = false;
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < array.Length; i++)
		{
			stringBuilder.Append(array[i]);
			if (i >= array.Length - 1)
			{
				continue;
			}
			string text = BbcodeTag.Replace(array[i], "").Trim();
			string text2 = BbcodeTag.Replace(array[i + 1], "").Trim();
			if (IsKeywordLine(text) && IsKeywordLine(text2))
			{
				bool num = text.EndsWith("。");
				bool flag2 = text2.EndsWith("。");
				if (!(num && flag2))
				{
					stringBuilder.Append(' ');
				}
				flag = true;
			}
			else
			{
				stringBuilder.Append('\n');
			}
		}
		if (flag)
		{
			result = stringBuilder.ToString();
		}
	}

	private static bool IsKeywordLine(string s)
	{
		if (string.IsNullOrEmpty(s))
		{
			return false;
		}
		if (s.EndsWith("。"))
		{
			return s.Length <= 6;
		}
		if (s.EndsWith(".") && s.Length <= 22)
		{
			return s.TrimEnd('.').Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries)
				.Length <= 2;
		}
		return false;
	}
}
