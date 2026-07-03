using System;
using System.Reflection;
using Danjin.Character;
using Danjin.Extensions;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;

namespace Danjin.Patches;

[HarmonyPatch]
[HarmonyPriority(600)]
internal class DanjinTonghuaTextIconPatch
{
	private static readonly string _iconPath = "Charui/tonghua_text_icon.png".ImagePath();

	internal static readonly string TonghuaIconBBCode = "[img]" + _iconPath + "[/img]";

	internal const string TonghuaTag = "[color=#E84393]彤华[/color]";

	internal const string TonghuaTagLegacy = "[gold]彤华[/gold]";

	private const string DefaultStarIcon = "[img]res://images/packed/sprite_fonts/star_icon.png[/img]";

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
		if (__instance.Pool is DanjinCardPool)
		{
			__result = __result.Replace("[color=#E84393]彤华[/color]", TonghuaIconBBCode);
			__result = __result.Replace("[gold]彤华[/gold]", TonghuaIconBBCode);
			__result = __result.Replace("[img]res://images/packed/sprite_fonts/star_icon.png[/img]", TonghuaIconBBCode);
		}
	}
}
