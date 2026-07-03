using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Cards;

namespace Danjin.Patches;

[HarmonyPatch(/*Could not decode attribute arguments.*/)]
internal static class NCardModulateGuardPatch
{
	[HarmonyPostfix]
	private static void Postfix(NCard __instance)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		((CanvasItem)__instance).Modulate = Colors.White;
	}
}
