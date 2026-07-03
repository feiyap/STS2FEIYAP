using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace Danjin.Patches;

[HarmonyPatch(typeof(NHealthBar), "RefreshForeground")]
public static class DanjinHpBarFillPatch
{
	[HarmonyPostfix]
	public static void Postfix(NHealthBar __instance)
	{
		DanjinHpBarRenderer.RebuildDanjinHpBar(__instance);
	}
}
