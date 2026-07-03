using System.Collections.Generic;
using Danjin.Character;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Nodes.Screens.TreasureRoomRelic;
using MegaCrit.Sts2.Core.Runs;

namespace Danjin.Patches;

internal static class DanjinSinglePlayerHandHelper
{
	public static bool IsSinglePlayerDanjin(IRunState? runState)
	{
		if (runState == null)
		{
			return false;
		}
		if (((IPlayerCollection)runState).Players.Count != 1)
		{
			return false;
		}
		Player val = ((IPlayerCollection)runState).Players[0];
		if (val == null)
		{
			return false;
		}
		return val.Character is Danjin.Character.Danjin;
	}

	public static IRunState? GetRunState(NHandImageCollection collection)
	{
		return Traverse.Create((object)collection).Field("_runState").GetValue<IRunState>();
	}

	public static List<NHandImage>? GetHands(NHandImageCollection collection)
	{
		return Traverse.Create((object)collection).Field("_hands").GetValue<List<NHandImage>>();
	}
}
