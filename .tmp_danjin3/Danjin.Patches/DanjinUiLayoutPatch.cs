using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using Danjin.Character;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace Danjin.Patches;

[HarmonyPatch(typeof(NCreatureStateDisplay), "SetCreatureBounds")]
public static class DanjinUiLayoutPatch
{
	private class YBase
	{
		public bool Captured;

		public float Y;
	}

	private const float GROUP_LIFT_Y = -6f;

	private const float HEALTHBAR_OFFSET_X = 0f;

	private const float HEALTHBAR_OFFSET_Y = 0f;

	private const float BLOCK_OFFSET_X = -18f;

	private const float BLOCK_OFFSET_Y = 10f;

	private const float NAME_OFFSET_X = 0f;

	private const float NAME_OFFSET_Y = 22f;

	private const float POWER_OFFSET_X = -8f;

	private const float POWER_OFFSET_Y = 22f;

	private static readonly FieldInfo? CsdCreatureField = AccessTools.Field(typeof(NCreatureStateDisplay), "_creature");

	private static readonly FieldInfo? CsdHealthBarField = AccessTools.Field(typeof(NCreatureStateDisplay), "_healthBar");

	private static readonly FieldInfo? CsdNameplateField = AccessTools.Field(typeof(NCreatureStateDisplay), "_nameplateContainer");

	private static readonly FieldInfo? CsdPowerField = AccessTools.Field(typeof(NCreatureStateDisplay), "_powerContainer");

	private static readonly FieldInfo? HpBlockField = AccessTools.Field(typeof(NHealthBar), "_blockContainer");

	private static readonly FieldInfo? HpOrigBlockField = AccessTools.Field(typeof(NHealthBar), "_originalBlockPosition");

	private static readonly FieldInfo? PowerOrigField = AccessTools.Field(typeof(NPowerContainer), "_originalPosition");

	private static readonly ConditionalWeakTable<Control, YBase> _baseY = new ConditionalWeakTable<Control, YBase>();

	private static float GetOrCaptureBaseY(Control ctrl)
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		YBase value = _baseY.GetValue(ctrl, (Control _) => new YBase());
		if (!value.Captured)
		{
			value.Y = ctrl.Position.Y;
			value.Captured = true;
		}
		return value.Y;
	}

	[HarmonyPostfix]
	public static void Postfix(NCreatureStateDisplay __instance)
	{
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_016c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0191: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			object? obj = CsdCreatureField?.GetValue(__instance);
			Creature val = (Creature)((obj is Creature) ? obj : null);
			if (val == null)
			{
				return;
			}
			object obj2;
			if (val == null)
			{
				obj2 = null;
			}
			else
			{
				Player player = val.Player;
				obj2 = ((player != null) ? player.Character : null);
			}
			if (!(obj2 is Danjin.Character.Danjin))
			{
				return;
			}
			object? obj3 = CsdHealthBarField?.GetValue(__instance);
			NHealthBar val2 = (NHealthBar)((obj3 is NHealthBar) ? obj3 : null);
			if (val2 != null)
			{
				float orCaptureBaseY = GetOrCaptureBaseY((Control)(object)val2);
				((Control)val2).Position = new Vector2(((Control)val2).Position.X + 0f, orCaptureBaseY + 0f + -6f);
			}
			object? obj4 = CsdNameplateField?.GetValue(__instance);
			Control val3 = (Control)((obj4 is Control) ? obj4 : null);
			if (val3 != null)
			{
				float orCaptureBaseY2 = GetOrCaptureBaseY(val3);
				Vector2 position = val3.Position;
				val3.Position = new Vector2(position.X + 0f, orCaptureBaseY2 + 22f);
			}
			if (val2 != null)
			{
				object? obj5 = HpBlockField?.GetValue(val2);
				Control val4 = (Control)((obj5 is Control) ? obj5 : null);
				if (val4 != null)
				{
					float orCaptureBaseY3 = GetOrCaptureBaseY(val4);
					Vector2 val5 = default(Vector2);
					((Vector2)(ref val5))._002Ector(val4.Position.X + -18f, orCaptureBaseY3 + 10f);
					val4.Position = val5;
					HpOrigBlockField?.SetValue(val2, val5);
				}
			}
			object? obj6 = CsdPowerField?.GetValue(__instance);
			NPowerContainer val6 = (NPowerContainer)((obj6 is NPowerContainer) ? obj6 : null);
			if (val6 != null)
			{
				float orCaptureBaseY4 = GetOrCaptureBaseY((Control)(object)val6);
				Vector2 val7 = default(Vector2);
				((Vector2)(ref val7))._002Ector(((Control)val6).Position.X + -8f, orCaptureBaseY4 + 22f + -6f);
				((Control)val6).Position = val7;
				PowerOrigField?.SetValue(val6, val7);
			}
		}
		catch (Exception value)
		{
			Log.Error($"[Danjin] UI 布局补丁失败:{value}", 2);
		}
	}
}
