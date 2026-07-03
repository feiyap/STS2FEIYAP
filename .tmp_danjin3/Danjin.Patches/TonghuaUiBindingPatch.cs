using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using Danjin.Character;
using Danjin.Resources;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace Danjin.Patches;

internal static class TonghuaUiBindingPatch
{
	[HarmonyPatch(typeof(NStarCounter), "ConnectStarsChangedSignal")]
	private static class ConnectStarsChangedSignalPatch
	{
		[HarmonyPrefix]
		private static bool Prefix(NStarCounter __instance)
		{
			if (!_reflectionOk)
			{
				return true;
			}
			if (!IsTonghuaCounter(__instance))
			{
				return true;
			}
			Player player = GetPlayer(__instance);
			if (player == null)
			{
				return false;
			}
			if ((bool)F_isListening.GetValue(__instance))
			{
				return false;
			}
			TonghuaState state = TonghuaCmd.GetState(player);
			Action<int, int> value = delegate(int oldVal, int newVal)
			{
				try
				{
					M_OnStarsChanged.Invoke(__instance, new object[2] { oldVal, newVal });
				}
				catch (Exception value2)
				{
					Log.Error($"[Danjin] 转发 TonghuaChanged → OnStarsChanged 失败: {value2}", 2);
				}
			};
			state.TonghuaChanged += value;
			_handlers.AddOrUpdate(__instance, value);
			F_isListening.SetValue(__instance, true);
			return false;
		}
	}

	[HarmonyPatch(typeof(NStarCounter), "_ExitTree")]
	private static class ExitTreePatch
	{
		[HarmonyPrefix]
		private static bool Prefix(NStarCounter __instance)
		{
			if (!_reflectionOk)
			{
				return true;
			}
			if (!IsTonghuaCounter(__instance))
			{
				return true;
			}
			if ((bool)F_isListening.GetValue(__instance) && _handlers.TryGetValue(__instance, out var value))
			{
				try
				{
					Player player = GetPlayer(__instance);
					if (player != null)
					{
						TonghuaCmd.GetState(player).TonghuaChanged -= value;
					}
				}
				catch (Exception value2)
				{
					Log.Error($"[Danjin] 解订阅 TonghuaChanged 失败: {value2}", 2);
				}
				_handlers.Remove(__instance);
				F_isListening.SetValue(__instance, false);
			}
			return true;
		}
	}

	[HarmonyPatch(typeof(NStarCounter), "_Process")]
	private static class ProcessPatch
	{
		[HarmonyPrefix]
		private static bool Prefix(NStarCounter __instance, double delta)
		{
			if (!_reflectionOk)
			{
				return true;
			}
			if (!IsTonghuaCounter(__instance))
			{
				return true;
			}
			Player player = GetPlayer(__instance);
			if (player == null)
			{
				return true;
			}
			int value = TonghuaCmd.GetState(player).Value;
			object? value2 = F_rotationLayers.GetValue(__instance);
			Control val = (Control)((value2 is Control) ? value2 : null);
			if (val != null)
			{
				float num = ((value == 0) ? 5f : 30f);
				int childCount = ((Node)val).GetChildCount(false);
				for (int i = 0; i < childCount; i++)
				{
					Node child = ((Node)val).GetChild(i, false);
					Control val2 = (Control)(object)((child is Control) ? child : null);
					if (val2 != null)
					{
						val2.RotationDegrees += (float)delta * num * (float)(i + 1);
					}
				}
			}
			float num2 = (float)F_lerpingStarCount.GetValue(__instance);
			float num3 = (float)F_velocity.GetValue(__instance);
			num2 = MathHelper.SmoothDamp(num2, (float)value, ref num3, 0.1f, (float)delta, float.PositiveInfinity);
			F_lerpingStarCount.SetValue(__instance, num2);
			F_velocity.SetValue(__instance, num3);
			try
			{
				M_SetStarCountText.Invoke(__instance, new object[1] { Mathf.RoundToInt(num2) });
			}
			catch (Exception value3)
			{
				Log.Error($"[Danjin] 调用 SetStarCountText 失败: {value3}", 2);
			}
			return false;
		}
	}

	[HarmonyPatch(typeof(NStarCounter), "RefreshVisibility")]
	private static class RefreshVisibilityPatch
	{
		[HarmonyPrefix]
		private static bool Prefix(NStarCounter __instance)
		{
			if (!_reflectionOk)
			{
				return true;
			}
			if (!IsTonghuaCounter(__instance))
			{
				return true;
			}
			Player player = GetPlayer(__instance);
			if (player == null)
			{
				return true;
			}
			int value = TonghuaCmd.GetState(player).Value;
			bool flag = player.Character is Danjin.Character.Danjin;
			((CanvasItem)__instance).Visible = ((CanvasItem)__instance).Visible || flag || value > 0;
			return false;
		}
	}

	private static readonly FieldInfo? F_player = AccessTools.Field(typeof(NStarCounter), "_player");

	private static readonly FieldInfo? F_isListening = AccessTools.Field(typeof(NStarCounter), "_isListeningToCombatState");

	private static readonly FieldInfo? F_rotationLayers = AccessTools.Field(typeof(NStarCounter), "_rotationLayers");

	private static readonly FieldInfo? F_lerpingStarCount = AccessTools.Field(typeof(NStarCounter), "_lerpingStarCount");

	private static readonly FieldInfo? F_velocity = AccessTools.Field(typeof(NStarCounter), "_velocity");

	private static readonly MethodInfo? M_OnStarsChanged = AccessTools.Method(typeof(NStarCounter), "OnStarsChanged", (Type[])null, (Type[])null);

	private static readonly MethodInfo? M_SetStarCountText = AccessTools.Method(typeof(NStarCounter), "SetStarCountText", (Type[])null, (Type[])null);

	private static readonly bool _reflectionOk = F_player != null && F_isListening != null && F_rotationLayers != null && F_lerpingStarCount != null && F_velocity != null && M_OnStarsChanged != null && M_SetStarCountText != null;

	private static readonly ConditionalWeakTable<NStarCounter, object> _tonghuaInstances = new ConditionalWeakTable<NStarCounter, object>();

	private static readonly ConditionalWeakTable<NStarCounter, Action<int, int>> _handlers = new ConditionalWeakTable<NStarCounter, Action<int, int>>();

	public static void MarkAsTonghuaCounter(NStarCounter counter)
	{
		if (counter != null)
		{
			_tonghuaInstances.AddOrUpdate(counter, new object());
		}
	}

	internal static bool IsTonghuaCounter(NStarCounter counter)
	{
		object value;
		if (counter != null)
		{
			return _tonghuaInstances.TryGetValue(counter, out value);
		}
		return false;
	}

	private static Player? GetPlayer(NStarCounter counter)
	{
		object? obj = F_player?.GetValue(counter);
		return (Player?)((obj is Player) ? obj : null);
	}
}
