using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History;
using MegaCrit.Sts2.Core.Entities.Cards;

namespace Danjin.Patches;

[HarmonyPatch(typeof(CombatHistory), "CardPlayFinished")]
internal class TrackCardPlayFinishPatch
{
	[HarmonyPostfix]
	private static void Postfix(CombatState combatState, CardPlay cardPlay)
	{
		if (CardContext.CurrentPlayingCard == cardPlay.Card)
		{
			CardContext.CurrentPlayingCard = null;
		}
	}
}
