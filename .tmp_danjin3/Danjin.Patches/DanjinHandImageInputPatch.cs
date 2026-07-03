using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Nodes.Screens.TreasureRoomRelic;

namespace Danjin.Patches;

[HarmonyPatch(typeof(NHandImageCollection), "_Input")]
public static class DanjinHandImageInputPatch
{
	[HarmonyPrefix]
	public static bool Prefix(NHandImageCollection __instance, InputEvent inputEvent)
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Invalid comparison between Unknown and I8
		if (!DanjinSinglePlayerHandHelper.IsSinglePlayerDanjin(DanjinSinglePlayerHandHelper.GetRunState(__instance)))
		{
			return true;
		}
		if (!LocalContext.NetId.HasValue)
		{
			return false;
		}
		NHandImage hand = __instance.GetHand(LocalContext.NetId.Value);
		if (hand == null)
		{
			return false;
		}
		if (inputEvent is InputEventMouseMotion)
		{
			hand.SetPointingPosition(((CanvasItem)__instance).GetGlobalMousePosition());
		}
		else
		{
			InputEventMouseButton val = (InputEventMouseButton)(object)((inputEvent is InputEventMouseButton) ? inputEvent : null);
			if (val != null && (long)val.ButtonIndex == 1)
			{
				if (((InputEvent)val).IsPressed() && !hand.IsDown)
				{
					hand.SetIsDown(true);
				}
				else if (((InputEvent)val).IsReleased() && hand.IsDown)
				{
					hand.SetIsDown(false);
				}
			}
		}
		return false;
	}
}
