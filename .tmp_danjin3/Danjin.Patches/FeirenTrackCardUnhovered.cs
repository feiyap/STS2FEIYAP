using HarmonyLib;
using MegaCrit.Sts2.Core.Multiplayer.Game.PeerInput;

namespace Danjin.Patches;

[HarmonyPatch(typeof(HoveredModelTracker), "OnLocalCardUnhovered")]
internal static class FeirenTrackCardUnhovered
{
	[HarmonyPostfix]
	private static void Postfix()
	{
		FeirenHealthForecast.ClearHoveredCard();
	}
}
