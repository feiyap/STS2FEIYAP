using System;
using Danjin.Utils;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Nodes.Screens.RunHistoryScreen;

namespace Danjin.Patches;

[HarmonyPatch(typeof(NRunHistoryPlayerIcon), "_Ready")]
internal static class DanjinRunHistoryIconScalePatch
{
	private static bool _failureReported;

	[HarmonyPostfix]
	private static void Postfix(NRunHistoryPlayerIcon __instance)
	{
		try
		{
			TextureRect nodeOrNull = ((Node)__instance).GetNodeOrNull<TextureRect>(NodePath.op_Implicit("%Icon"));
			if (nodeOrNull == null)
			{
				ReportFailureOnce("RunHistoryPlayerIcon 内找不到 %Icon 节点, 跳过头像缩放修复");
				return;
			}
			nodeOrNull.ExpandMode = (ExpandModeEnum)1;
			nodeOrNull.StretchMode = (StretchModeEnum)5;
			DanjinLog.Verbose("[Danjin] RunHistoryIcon 已修正 ExpandMode/StretchMode");
		}
		catch (Exception ex)
		{
			ReportFailureOnce("RunHistoryIconScalePatch Postfix 抛异常: " + ex.GetType().Name + ": " + ex.Message);
		}
	}

	private static void ReportFailureOnce(string msg)
	{
		if (!_failureReported)
		{
			_failureReported = true;
			Log.Warn("[Danjin] " + msg + " (后续相同错误将静默)", 2);
		}
	}
}
