using System;
using System.Reflection;
using Danjin.Powers;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;

namespace Danjin.Patches;

[HarmonyPatch]
internal static class HuangHuangYouYouMarkDescription
{
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
	private static void Postfix(CardModel __instance, object previewType, ref string __result)
	{
		try
		{
			if (previewType?.ToString() == "Upgrade" || !CombatManager.Instance.IsInProgress || !((AbstractModel)__instance).IsMutable)
			{
				return;
			}
			Player owner = __instance.Owner;
			Creature val = ((owner != null) ? owner.Creature : null);
			if (val == null)
			{
				return;
			}
			HuangHuangYouYouPower power = val.GetPower<HuangHuangYouYouPower>();
			if (power != null)
			{
				int markCount = power.GetMarkCount(__instance);
				if (markCount > 0)
				{
					string text = __result ?? string.Empty;
					string text2 = $"[green]打出时额外抽 {markCount} 张牌。[/green]";
					__result = (string.IsNullOrEmpty(text) ? text2 : (text + "\n" + text2));
				}
			}
		}
		catch (Exception value)
		{
			Log.Error($"[DanjinMod] HuangHuangYouYouMarkDescription.Postfix 异常: {value}", 1);
		}
	}
}
