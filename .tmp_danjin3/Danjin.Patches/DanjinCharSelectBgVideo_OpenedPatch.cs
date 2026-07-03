using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect;

namespace Danjin.Patches;

[HarmonyPatch(typeof(NCharacterSelectScreen), "OnSubmenuOpened")]
public static class DanjinCharSelectBgVideo_OpenedPatch
{
	[HarmonyPostfix]
	public static void Postfix(NCharacterSelectScreen __instance)
	{
		DanjinCharSelectBgVideo.TryTakeOver(__instance);
	}
}
