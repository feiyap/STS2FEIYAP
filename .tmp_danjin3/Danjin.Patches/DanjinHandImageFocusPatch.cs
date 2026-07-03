using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.Screens.TreasureRoomRelic;

namespace Danjin.Patches;

[HarmonyPatch(typeof(NHandImageCollection), "ProcessGuiFocus")]
public static class DanjinHandImageFocusPatch
{
	[HarmonyPrefix]
	public static bool Prefix(NHandImageCollection __instance, Control focusedControl)
	{
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		if (!DanjinSinglePlayerHandHelper.IsSinglePlayerDanjin(DanjinSinglePlayerHandHelper.GetRunState(__instance)))
		{
			return true;
		}
		if (!((CanvasItem)__instance).IsVisibleInTree())
		{
			return false;
		}
		if (NControllerManager.Instance == null || !NControllerManager.Instance.IsUsingController)
		{
			return false;
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
		if (focusedControl is NTreasureRoomRelicHolder)
		{
			Vector2 down = Vector2.Down;
			Vector2 val = ((Vector2)(ref down)).Rotated(((Control)hand).Rotation);
			Vector2 pointingPosition = focusedControl.GlobalPosition + focusedControl.Size * 0.5f + val * 100f;
			hand.SetPointingPosition(pointingPosition);
		}
		else
		{
			hand.AnimateAway();
		}
		return false;
	}
}
