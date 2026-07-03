using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History;
using MegaCrit.Sts2.Core.Entities.Cards;

namespace Danjin.Patches;

[HarmonyPatch(typeof(CombatHistory), "CardPlayStarted")]
internal class TrackCardPlayStartPatch
{
	[HarmonyPostfix]
	private static void Postfix(CombatState combatState, CardPlay cardPlay)
	{
		CardContext.CurrentPlayingCard = cardPlay.Card;
	}
}
