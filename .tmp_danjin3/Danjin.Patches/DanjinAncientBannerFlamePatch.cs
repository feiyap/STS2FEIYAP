using System;
using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;

namespace Danjin.Patches;

[HarmonyPatch(typeof(NCard), "Reload")]
internal static class DanjinAncientBannerFlamePatch
{
	private static readonly FieldInfo? _ancientBannerField = AccessTools.Field(typeof(NCard), "_ancientBanner");

	[HarmonyPostfix]
	private static void Postfix(NCard __instance)
	{
		if (_ancientBannerField == null)
		{
			return;
		}
		try
		{
			CardModel model = __instance.Model;
			if (model == null)
			{
				return;
			}
			object? value = _ancientBannerField.GetValue(__instance);
			Control val = (Control)((value is Control) ? value : null);
			CanvasItem val2 = ((val != null) ? ((Node)val).GetNodeOrNull<CanvasItem>(NodePath.op_Implicit("Fire")) : null);
			if (!DanjinAncientLook.IsDanjinCard(model))
			{
				if (val != null && (object)((CanvasItem)val).Material == DanjinAncientLook.BannerMat())
				{
					((CanvasItem)val).Material = null;
				}
				if (val2 != null && (object)val2.Material == DanjinAncientLook.FlameMat())
				{
					val2.Material = null;
				}
			}
			else
			{
				if (val != null)
				{
					((CanvasItem)val).Material = (Material)(object)DanjinAncientLook.BannerMat();
				}
				if (val2 != null)
				{
					val2.Material = (Material)(object)DanjinAncientLook.FlameMat();
				}
			}
		}
		catch (Exception ex)
		{
			Log.Error("[Danjin] Ancient缎带/火焰染色异常: " + ex.Message, 2);
		}
	}
}
