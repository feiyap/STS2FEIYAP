using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Multiplayer.Game.PeerInput;

namespace Danjin.Patches;

[HarmonyPatch(typeof(HoveredModelTracker), "OnLocalCardSelected")]
internal static class FeirenTrackCardSelected
{
	[HarmonyPostfix]
	private static void Postfix(CardModel cardModel)
	{
		FeirenHealthForecast.SetHoveredCard(cardModel);
	}
}
