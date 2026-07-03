using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Nodes.Events.Custom;
using MegaCrit.Sts2.Core.Nodes.Screens.Shops;

namespace Danjin.Patches;

[HarmonyPatch(typeof(NFakeMerchant), "AfterRoomIsLoaded")]
internal static class NFakeMerchantDanjinVisualScalePatch
{
	private const float CompensationScale = 0.5714286f;

	[HarmonyPostfix]
	private static void CompensateNonSpineVisualScale(NFakeMerchant __instance)
	{
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		Control nodeOrNull = ((Node)__instance).GetNodeOrNull<Control>(NodePath.op_Implicit("%CharacterContainer"));
		if (nodeOrNull == null)
		{
			Log.Debug("[Danjin] NFakeMerchant: %CharacterContainer not found, skip scale compensation.", 2);
			return;
		}
		int num = 0;
		int num2 = 0;
		foreach (Node child2 in ((Node)nodeOrNull).GetChildren(false))
		{
			NMerchantCharacter val = (NMerchantCharacter)(object)((child2 is NMerchantCharacter) ? child2 : null);
			if (val == null)
			{
				continue;
			}
			num2++;
			if (((Node)val).GetChildCount(false) != 0)
			{
				Node child = ((Node)val).GetChild(0, false);
				if (child != null && !(((object)child).GetType().Name == "SpineSprite"))
				{
					((Node2D)val).Scale = ((Node2D)val).Scale * 0.5714286f;
					num++;
				}
			}
		}
		if (num > 0)
		{
			Log.Debug($"[Danjin] NFakeMerchant: applied 1/1.75 scale compensation to {num}/{num2} merchant character(s).", 2);
		}
	}
}
