using Danjin.Character;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect;

namespace Danjin.Patches;

[HarmonyPatch(typeof(NCharacterSelectScreen), "SelectCharacter")]
public static class DanjinCharSelectBgVideo_SelectPatch
{
	[HarmonyPostfix]
	public static void Postfix(NCharacterSelectScreen __instance, CharacterModel characterModel)
	{
		if (characterModel is Danjin.Character.Danjin)
		{
			DanjinCharSelectBgVideo.TryTakeOver(__instance);
		}
	}
}
