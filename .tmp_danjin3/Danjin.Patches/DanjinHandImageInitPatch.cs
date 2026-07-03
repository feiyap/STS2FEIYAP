using System;
using System.Collections.Generic;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Multiplayer.Game.PeerInput;
using MegaCrit.Sts2.Core.Nodes.Screens.TreasureRoomRelic;
using MegaCrit.Sts2.Core.Runs;

namespace Danjin.Patches;

[HarmonyPatch(typeof(NHandImageCollection), "Initialize")]
public static class DanjinHandImageInitPatch
{
	[HarmonyPostfix]
	public static void Postfix(NHandImageCollection __instance, IRunState runState)
	{
		if (!DanjinSinglePlayerHandHelper.IsSinglePlayerDanjin(runState))
		{
			return;
		}
		List<NHandImage> hands = DanjinSinglePlayerHandHelper.GetHands(__instance);
		if (hands != null && hands.Count > 0)
		{
			return;
		}
		Traverse val = Traverse.Create((object)__instance);
		RunManager instance = RunManager.Instance;
		PeerInputSynchronizer val2 = ((instance != null) ? instance.InputSynchronizer : null);
		if (val2 == null)
		{
			return;
		}
		val.Field("_synchronizer").SetValue((object)val2);
		foreach (Player player in ((IPlayerCollection)runState).Players)
		{
			val.Method("AddHand", new object[1] { player.NetId }).GetValue();
		}
		val.Method("UpdateHandVisibility", Array.Empty<object>()).GetValue();
	}
}
