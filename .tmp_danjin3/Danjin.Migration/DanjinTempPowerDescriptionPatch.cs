using Danjin.Powers;
using HarmonyLib;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;

namespace Danjin.Migration;

[HarmonyPatch(typeof(PowerModel), "AddDumbVariablesToDescription")]
internal static class DanjinTempPowerDescriptionPatch
{
	[HarmonyPostfix]
	private static void Postfix(PowerModel __instance, LocString description)
	{
		if (__instance is IDanjinTempPower { InternallyAppliedPower: not null } danjinTempPower)
		{
			description.Add("TemporaryPowerTitle", danjinTempPower.InternallyAppliedPower.Title);
		}
	}
}
