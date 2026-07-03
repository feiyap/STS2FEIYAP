using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Multiplayer.Game.PeerInput;

namespace Danjin.Patches;

[HarmonyPatch(typeof(HoveredModelTracker), "OnLocalCardHovered")]
internal static class FeirenTrackCardHovered
{
	[HarmonyPostfix]
	private static void Postfix(CardModel cardModel)
	{
		FeirenHealthForecast.SetHoveredCard(cardModel);
	}
}
