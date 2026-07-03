using System;
using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;

namespace Danjin.Patches;

[HarmonyPatch(typeof(NCard), "UpdateTypePlaque")]
internal static class DanjinAncientPlaquePatch
{
	private static readonly FieldInfo? _typePlaqueField = AccessTools.Field(typeof(NCard), "_typePlaque");

	[HarmonyPostfix]
	private static void Postfix(NCard __instance)
	{
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Invalid comparison between Unknown and I4
		if (_typePlaqueField == null)
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
			object? value = _typePlaqueField.GetValue(__instance);
			Control val = (Control)((value is Control) ? value : null);
			if (val == null)
			{
				return;
			}
			if (!DanjinAncientLook.IsDanjinCard(model))
			{
				if ((object)((CanvasItem)val).Material == DanjinAncientLook.PlaqueMat())
				{
					((CanvasItem)val).Material = null;
				}
			}
			else if ((int)model.Rarity == 5)
			{
				((CanvasItem)val).Material = (Material)(object)DanjinAncientLook.PlaqueMat();
			}
		}
		catch (Exception ex)
		{
			Log.Error("[Danjin] Ancient类型铭牌染色异常: " + ex.Message, 2);
		}
	}
}
