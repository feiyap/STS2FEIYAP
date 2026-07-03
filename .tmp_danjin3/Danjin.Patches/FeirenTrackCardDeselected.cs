using HarmonyLib;
using MegaCrit.Sts2.Core.Multiplayer.Game.PeerInput;

namespace Danjin.Patches;

[HarmonyPatch(typeof(HoveredModelTracker), "OnLocalCardDeselected")]
internal static class FeirenTrackCardDeselected
{
	[HarmonyPostfix]
	private static void Postfix()
	{
		FeirenHealthForecast.ClearHoveredCard();
	}
}
