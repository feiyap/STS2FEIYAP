using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace Danjin.Patches;

[HarmonyPatch(typeof(NCardPlay), "CannotPlayThisCardFtueCheck")]
internal static class CardPlayAttemptCapturePatch
{
	[HarmonyPostfix]
	private static void Postfix(CardModel card)
	{
		CardPlayAttemptContext.LastFailedCard = card;
	}
}
