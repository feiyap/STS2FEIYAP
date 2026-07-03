using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using Danjin.Character;
using Danjin.Extensions;
using Danjin.Resources;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace Danjin.Patches;

internal static class DanjinHpBarRenderer
{
	private class TweenHolder
	{
		public Tween? Tween;

		public float LastTargetW = -1f;
	}

	private class BoxedBool
	{
		public bool Value;
	}

	private class LabelOffsetHolder
	{
		public bool Captured;

		public float OrigOffsetTop;

		public float OrigOffsetBottom;
	}

	private const string FillTextureFileName = "danjin_hp_fill.png";

	private const string FrameTextureFileName = "danjin_hp_frame.png";

	private const string TroughTextureFileName = "danjin_hp_trough.png";

	private const string BlockTextureFileName = "danjin_hp_block.png";

	private const string DoomTextureFileName = "danjin_hp_doom.png";

	private const string DamageTextureFileName = "danjin_hp_damage.png";

	private const string TransparentFillTextureFileName = "danjin_hp_fill_transparent.png";

	private const string TroughRectName = "DanjinTroughRect";

	private const string BlockRectName = "DanjinBlockRect";

	private const string FillRectName = "DanjinFillRect";

	private const string DoomRectName = "DanjinDoomRect";

	private const string FrameOverlayName = "DanjinFrameOverlay";

	private const string DamageRectName = "DanjinDamageRect";

	private const string TransparentFillRectName = "DanjinTransparentFillRect";

	private const string LegacyBlockOverlayName = "DanjinBlockOverlay";

	private const string LegacyDoomOverlayName = "DanjinDoomOverlay";

	private const float FRAME_TEX_W = 994f;

	private const float FRAME_TEX_H = 263f;

	private const float FRAME_ASPECT = 0.26458752f;

	private const float FRAME_EXTEND_X = 12f;

	private const float FRAME_Y_OFFSET = 16f;

	private const float FRAME_SCALE_X = 1f;

	private const float FRAME_SCALE_Y = 0.7f;

	private const bool FRAME_ON_TOP = true;

	private const float FILL_INSET_X_LEFT_RATIO = 0.05f;

	private const float FILL_INSET_X_RIGHT_RATIO = 0.02f;

	private const float FILL_INSET_TOP_RATIO = 0.17f;

	private const float FILL_INSET_BOTTOM_RATIO = 0.3f;

	private const float LABEL_Y_OFFSET = 12f;

	private const double FILL_TWEEN_DURATION = 0.15;

	private const double DAMAGE_TWEEN_DELAY = 1.0;

	private const double DAMAGE_TWEEN_DURATION = 1.0;

	private const double DAMAGE_HEAL_DURATION = 0.15;

	private const TransitionType DAMAGE_TWEEN_TRANS = (TransitionType)5L;

	private static Texture2D? _cachedFillTex;

	private static Texture2D? _cachedFrameTex;

	private static Texture2D? _cachedTroughTex;

	private static Texture2D? _cachedBlockTex;

	private static Texture2D? _cachedDoomTex;

	private static Texture2D? _cachedDamageTex;

	private static Texture2D? _cachedTransparentFillTex;

	private static readonly ConditionalWeakTable<Control, TweenHolder> _fillTweens = new ConditionalWeakTable<Control, TweenHolder>();

	private static readonly ConditionalWeakTable<Control, TweenHolder> _damageTweens = new ConditionalWeakTable<Control, TweenHolder>();

	private static readonly Color TransparentWhite = new Color(1f, 1f, 1f, 0f);

	private static readonly HashSet<string> OurOverlayNames = new HashSet<string> { "DanjinTroughRect", "DanjinBlockRect", "DanjinFillRect", "DanjinDoomRect", "DanjinDamageRect", "DanjinTransparentFillRect", "DanjinFrameOverlay" };

	private static readonly ConditionalWeakTable<NHealthBar, BoxedBool> _troughHidden = new ConditionalWeakTable<NHealthBar, BoxedBool>();

	private static readonly ConditionalWeakTable<NHealthBar, BoxedBool> _legacyCleaned = new ConditionalWeakTable<NHealthBar, BoxedBool>();

	private static readonly FieldInfo? _stateDisplayField = AccessTools.Field(typeof(NCreature), "_stateDisplay");

	private static readonly FieldInfo? _healthBarFieldOnStateDisplay = AccessTools.Field(typeof(NCreatureStateDisplay), "_healthBar");

	private static readonly ConditionalWeakTable<Control, LabelOffsetHolder> _labelOffsets = new ConditionalWeakTable<Control, LabelOffsetHolder>();

	public static int SelfDamageDepth { get; private set; }

	public static int SnapshottedVirtualMaxHp { get; private set; }

	public static void EnterSelfDamage(int snapshotVirtualMaxHp)
	{
		if (SelfDamageDepth == 0)
		{
			SnapshottedVirtualMaxHp = snapshotVirtualMaxHp;
		}
		SelfDamageDepth++;
	}

	public static void ExitSelfDamage()
	{
		if (SelfDamageDepth > 0)
		{
			SelfDamageDepth--;
		}
		if (SelfDamageDepth == 0)
		{
			SnapshottedVirtualMaxHp = 0;
		}
	}

	public static void RequestRefreshFor(Player? player)
	{
		try
		{
			if (player == null)
			{
				return;
			}
			NCombatRoom instance = NCombatRoom.Instance;
			if (instance == null)
			{
				return;
			}
			Creature creature = player.Creature;
			if (creature == null)
			{
				return;
			}
			NCreature creatureNode = instance.GetCreatureNode(creature);
			if (creatureNode == null)
			{
				return;
			}
			object? obj = _stateDisplayField?.GetValue(creatureNode);
			NCreatureStateDisplay val = (NCreatureStateDisplay)((obj is NCreatureStateDisplay) ? obj : null);
			if (val != null)
			{
				object? obj2 = _healthBarFieldOnStateDisplay?.GetValue(val);
				NHealthBar val2 = (NHealthBar)((obj2 is NHealthBar) ? obj2 : null);
				if (val2 != null)
				{
					val2.RefreshValues();
				}
			}
		}
		catch (Exception value)
		{
			Log.Error($"[Danjin] HpBar RequestRefreshFor 异常：{value}", 2);
		}
	}

	public static void RebuildDanjinHpBar(NHealthBar __instance)
	{
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_025a: Unknown result type (might be due to invalid IL or missing references)
		//IL_026a: Unknown result type (might be due to invalid IL or missing references)
		//IL_036e: Unknown result type (might be due to invalid IL or missing references)
		//IL_037e: Unknown result type (might be due to invalid IL or missing references)
		//IL_03db: Unknown result type (might be due to invalid IL or missing references)
		//IL_03eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_067f: Unknown result type (might be due to invalid IL or missing references)
		//IL_068f: Unknown result type (might be due to invalid IL or missing references)
		//IL_049c: Unknown result type (might be due to invalid IL or missing references)
		//IL_04a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_04c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_050f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0514: Unknown result type (might be due to invalid IL or missing references)
		//IL_0521: Unknown result type (might be due to invalid IL or missing references)
		//IL_08cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_08df: Unknown result type (might be due to invalid IL or missing references)
		//IL_0740: Unknown result type (might be due to invalid IL or missing references)
		//IL_074c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0751: Unknown result type (might be due to invalid IL or missing references)
		//IL_075e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0767: Unknown result type (might be due to invalid IL or missing references)
		//IL_0782: Unknown result type (might be due to invalid IL or missing references)
		//IL_0787: Unknown result type (might be due to invalid IL or missing references)
		//IL_0794: Unknown result type (might be due to invalid IL or missing references)
		//IL_058f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0594: Unknown result type (might be due to invalid IL or missing references)
		//IL_07f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_07fc: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			object? obj = AccessTools.Field(typeof(NHealthBar), "_creature")?.GetValue(__instance);
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
			object? obj3 = AccessTools.Field(typeof(NHealthBar), "_hpForeground")?.GetValue(__instance);
			Control val2 = (Control)((obj3 is Control) ? obj3 : null);
			if (val2 == null)
			{
				return;
			}
			object? obj4 = AccessTools.Field(typeof(NHealthBar), "_hpForegroundContainer")?.GetValue(__instance);
			Control val3 = (Control)((obj4 is Control) ? obj4 : null);
			if (val3 == null)
			{
				return;
			}
			HideVanillaVisuals(__instance, val2, val3);
			Control hpBarContainer = __instance.HpBarContainer;
			if (hpBarContainer == null)
			{
				return;
			}
			CleanupLegacyNodes(__instance, hpBarContainer);
			bool flag = val.CurrentHp <= 0;
			float x = hpBarContainer.Size.X;
			float y = hpBarContainer.Size.Y;
			float num = x + 24f;
			float num2 = num * 1f;
			float num3 = num * 0.26458752f * 0.7f;
			float num4 = (x - num2) / 2f;
			float num5 = (y - num3) / 2f + 16f;
			float num6 = num2 * 0.05f;
			float num7 = num2 * 0.02f;
			float num8 = num4 + num6;
			float num9 = num2 - num6 - num7;
			float num10 = num3 * 0.17f;
			float num11 = num3 * 0.3f;
			float num12 = num5 + num10;
			float num13 = num3 - num10 - num11;
			int num14 = Math.Max(1, val.MaxHp);
			int num15 = ((!flag) ? Math.Max(0, val.Block) : 0);
			int num16 = ((!flag && val.HasPower<DoomPower>()) ? Math.Max(0, val.GetPowerAmount<DoomPower>()) : 0);
			float num17 = Math.Clamp((float)val.CurrentHp / (float)num14, 0f, 1f);
			float val4 = num9 * num17;
			Texture2D val5 = LoadTextureSafe("danjin_hp_trough.png".CharacterUiPath(), ref _cachedTroughTex);
			TextureRect val6 = ((Node)hpBarContainer).GetNodeOrNull<TextureRect>(NodePath.op_Implicit("DanjinTroughRect"));
			if (val6 == null && !flag && val5 != null)
			{
				val6 = MakeBarTextureRect("DanjinTroughRect", val5);
				((Node)hpBarContainer).AddChild((Node)(object)val6, false, (InternalMode)0);
			}
			if (val6 != null)
			{
				((CanvasItem)val6).Visible = !flag && val5 != null;
				if (((CanvasItem)val6).Visible)
				{
					val6.Texture = val5;
					((Control)val6).Position = new Vector2(num8, num12);
					((Control)val6).Size = new Vector2(num9, num13);
				}
			}
			Texture2D val7 = LoadTextureSafe("danjin_hp_fill_transparent.png".CharacterUiPath(), ref _cachedTransparentFillTex);
			TextureRect val8 = ((Node)hpBarContainer).GetNodeOrNull<TextureRect>(NodePath.op_Implicit("DanjinTransparentFillRect"));
			int num18 = 0;
			Player player2 = val.Player;
			if (((player2 != null) ? player2.PlayerCombatState : null) != null)
			{
				num18 = Math.Max(0, TonghuaHealPoolCmd.PeekRemaining(val.Player));
			}
			bool flag2 = SelfDamageDepth > 0;
			int num19 = (flag2 ? SnapshottedVirtualMaxHp : Math.Min(val.CurrentHp + num18, num14));
			bool flag3 = !flag && val7 != null && (flag2 ? (num19 > val.CurrentHp) : (num18 > 0));
			if (val8 == null && flag3)
			{
				val8 = MakeBarTextureRect("DanjinTransparentFillRect", val7);
				((Node)hpBarContainer).AddChild((Node)(object)val8, false, (InternalMode)0);
			}
			if (val8 != null)
			{
				((CanvasItem)val8).Visible = flag3;
				if (flag3)
				{
					val8.Texture = val7;
					float num20 = (float)num19 / (float)num14;
					float num21 = Math.Clamp(num9 * num20, 1f, num9);
					((Control)val8).Position = new Vector2(num8, num12);
					((Control)val8).Size = new Vector2(num21, num13);
				}
			}
			TextureRect val9 = ((Node)hpBarContainer).GetNodeOrNull<TextureRect>(NodePath.op_Implicit("DanjinFrameOverlay"));
			if (val9 == null)
			{
				Texture2D val10 = LoadTextureSafe("danjin_hp_frame.png".CharacterUiPath(), ref _cachedFrameTex);
				if (val10 != null)
				{
					val9 = MakeBarTextureRect("DanjinFrameOverlay", val10);
					((Node)hpBarContainer).AddChild((Node)(object)val9, false, (InternalMode)0);
				}
			}
			if (val9 != null)
			{
				((Control)val9).Position = new Vector2(num4, num5);
				((Control)val9).Size = new Vector2(num2, num3);
			}
			Texture2D val11 = LoadTextureSafe("danjin_hp_damage.png".CharacterUiPath(), ref _cachedDamageTex);
			TextureRect val12 = ((Node)hpBarContainer).GetNodeOrNull<TextureRect>(NodePath.op_Implicit("DanjinDamageRect"));
			if (val12 == null && !flag && val11 != null)
			{
				val12 = MakeBarTextureRect("DanjinDamageRect", val11);
				((Node)hpBarContainer).AddChild((Node)(object)val12, false, (InternalMode)0);
			}
			if (val12 != null)
			{
				bool flag4 = !flag && val11 != null;
				((CanvasItem)val12).Visible = flag4 && num15 == 0;
				if (flag4)
				{
					val12.Texture = val11;
					float num22 = Math.Max(val4, 1f);
					float num23 = Math.Max(num13, 1f);
					((Control)val12).Position = new Vector2(num8, num12);
					Vector2 size = ((Control)val12).Size;
					size.Y = num23;
					((Control)val12).Size = size;
					float x2 = ((Control)val12).Size.X;
					TweenHolder value = _damageTweens.GetValue((Control)(object)val12, (Control _) => new TweenHolder());
					if (Math.Abs(x2 - num22) < 1f)
					{
						size = ((Control)val12).Size;
						size.X = num22;
						((Control)val12).Size = size;
						value.LastTargetW = num22;
					}
					else if (Math.Abs(value.LastTargetW - num22) > 0.5f)
					{
						bool flag5 = num22 < x2;
						Tween? tween = value.Tween;
						if (tween != null)
						{
							tween.Kill();
						}
						value.LastTargetW = num22;
						Tween val13 = ((Node)val12).CreateTween();
						PropertyTweener val14 = val13.TweenProperty((GodotObject)(object)val12, NodePath.op_Implicit("size"), Variant.op_Implicit(new Vector2(num22, num23)), flag5 ? 1.0 : 0.15).SetEase((EaseType)1).SetTrans((TransitionType)(flag5 ? 5 : 4));
						if (flag5)
						{
							val14.SetDelay(1.0);
						}
						value.Tween = val13;
					}
				}
			}
			Texture2D val15 = LoadTextureSafe("danjin_hp_block.png".CharacterUiPath(), ref _cachedBlockTex);
			TextureRect val16 = ((Node)hpBarContainer).GetNodeOrNull<TextureRect>(NodePath.op_Implicit("DanjinBlockRect"));
			if (val16 == null && !flag && num15 > 0 && val15 != null)
			{
				val16 = MakeBarTextureRect("DanjinBlockRect", val15);
				((Node)hpBarContainer).AddChild((Node)(object)val16, false, (InternalMode)0);
			}
			if (val16 != null && (((CanvasItem)val16).Visible = !flag && num15 > 0 && val15 != null))
			{
				val16.Texture = val15;
				float num24 = Math.Max(val4, 1f);
				((Control)val16).Position = new Vector2(num8, num12);
				((Control)val16).Size = new Vector2(num24, num13);
			}
			Texture2D val17 = LoadTextureSafe("danjin_hp_fill.png".CharacterUiPath(), ref _cachedFillTex);
			TextureRect val18 = ((Node)hpBarContainer).GetNodeOrNull<TextureRect>(NodePath.op_Implicit("DanjinFillRect"));
			if (val18 == null && !flag && val17 != null)
			{
				val18 = MakeBarTextureRect("DanjinFillRect", val17);
				((Node)hpBarContainer).AddChild((Node)(object)val18, false, (InternalMode)0);
			}
			if (val18 != null)
			{
				bool flag7 = !flag && val17 != null;
				((CanvasItem)val18).Visible = flag7 && num15 == 0;
				if (flag7)
				{
					val18.Texture = val17;
					float num25 = Math.Max(val4, 1f);
					float num26 = Math.Max(num13, 1f);
					((Control)val18).Position = new Vector2(num8, num12);
					Vector2 size2 = ((Control)val18).Size;
					size2.Y = num26;
					((Control)val18).Size = size2;
					if (Math.Abs(((Control)val18).Size.X - num25) < 1f)
					{
						size2 = ((Control)val18).Size;
						size2.X = num25;
						((Control)val18).Size = size2;
					}
					else
					{
						TweenHolder value2 = _fillTweens.GetValue((Control)(object)val18, (Control _) => new TweenHolder());
						Tween? tween2 = value2.Tween;
						if (tween2 != null)
						{
							tween2.Kill();
						}
						Tween val19 = ((Node)val18).CreateTween();
						val19.TweenProperty((GodotObject)(object)val18, NodePath.op_Implicit("size"), Variant.op_Implicit(new Vector2(num25, num26)), 0.15).SetEase((EaseType)1).SetTrans((TransitionType)4);
						value2.Tween = val19;
					}
				}
			}
			Texture2D val20 = LoadTextureSafe("danjin_hp_doom.png".CharacterUiPath(), ref _cachedDoomTex);
			TextureRect val21 = ((Node)hpBarContainer).GetNodeOrNull<TextureRect>(NodePath.op_Implicit("DanjinDoomRect"));
			if (val21 == null && !flag && num16 > 0 && val20 != null)
			{
				val21 = MakeBarTextureRect("DanjinDoomRect", val20);
				((Node)hpBarContainer).AddChild((Node)(object)val21, false, (InternalMode)0);
			}
			if (val21 != null && (((CanvasItem)val21).Visible = !flag && num16 > 0 && val20 != null))
			{
				val21.Texture = val20;
				float val22 = (float)num16 / (float)num14 * num9;
				val22 = Math.Min(val22, num9);
				val22 = Math.Max(val22, 1f);
				((Control)val21).Position = new Vector2(num8, num12);
				((Control)val21).Size = new Vector2(val22, num13);
			}
			EnsureOnTop(hpBarContainer, (Control?)(object)val6);
			EnsureOnTop(hpBarContainer, (Control?)(object)val8);
			EnsureOnTop(hpBarContainer, (Control?)(object)val12);
			EnsureOnTop(hpBarContainer, (Control?)(object)val16);
			EnsureOnTop(hpBarContainer, (Control?)(object)val18);
			EnsureOnTop(hpBarContainer, (Control?)(object)val21);
			EnsureOnTop(hpBarContainer, (Control?)(object)val9);
			Control nodeOrNull = ((Node)__instance).GetNodeOrNull<Control>(NodePath.op_Implicit("%HpLabel"));
			if (nodeOrNull != null)
			{
				nodeOrNull.MouseFilter = (MouseFilterEnum)2;
				Node parent = ((Node)nodeOrNull).GetParent();
				Control val23 = (Control)(object)((parent is Control) ? parent : null);
				if (val23 != null)
				{
					((Node)val23).MoveChild((Node)(object)nodeOrNull, ((Node)val23).GetChildCount(false) - 1);
				}
				ApplyHpLabelOffset(nodeOrNull, 12f);
				StripHpLabelShadow(nodeOrNull);
			}
		}
		catch (Exception value3)
		{
			Log.Error($"[Danjin] HpBar 自定义美术失败：{value3}", 2);
		}
	}

	private static TextureRect MakeBarTextureRect(string name, Texture2D? tex)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Expected O, but got Unknown
		return new TextureRect
		{
			Name = StringName.op_Implicit(name),
			Texture = tex,
			StretchMode = (StretchModeEnum)0,
			ExpandMode = (ExpandModeEnum)1,
			MouseFilter = (MouseFilterEnum)2,
			ClipContents = false,
			AnchorLeft = 0f,
			AnchorTop = 0f,
			AnchorRight = 0f,
			AnchorBottom = 0f
		};
	}

	private static void EnsureOnTop(Control container, Control? ctrl)
	{
		if (ctrl != null && (object)((Node)ctrl).GetParent() == container)
		{
			((Node)container).MoveChild((Node)(object)ctrl, ((Node)container).GetChildCount(false) - 1);
		}
	}

	private static void CleanupLegacyNodes(NHealthBar instance, Control container)
	{
		BoxedBool value = _legacyCleaned.GetValue(instance, (NHealthBar _) => new BoxedBool());
		if (!value.Value)
		{
			value.Value = true;
			QueueFreeIfPresent(container, "DanjinBlockOverlay");
			QueueFreeIfPresent(container, "DanjinDoomOverlay");
		}
	}

	private static void QueueFreeIfPresent(Control container, string nodeName)
	{
		Node nodeOrNull = ((Node)container).GetNodeOrNull(NodePath.op_Implicit(nodeName));
		if (nodeOrNull != null && !((GodotObject)nodeOrNull).IsQueuedForDeletion())
		{
			nodeOrNull.Name = StringName.op_Implicit($"{nodeName}_legacy_{GD.Randi()}");
			CanvasItem val = (CanvasItem)(object)((nodeOrNull is CanvasItem) ? nodeOrNull : null);
			if (val != null)
			{
				val.Visible = false;
			}
			nodeOrNull.QueueFree();
			Log.Debug("[Danjin] 已清理老版本残留节点 " + nodeName, 2);
		}
	}

	private static void StripHpLabelShadow(Control label)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			Label val = (Label)(object)((label is Label) ? label : null);
			if (val != null)
			{
				((Control)val).AddThemeColorOverride(StringName.op_Implicit("font_shadow_color"), new Color(0f, 0f, 0f, 0f));
				((Control)val).AddThemeConstantOverride(StringName.op_Implicit("shadow_offset_x"), 0);
				((Control)val).AddThemeConstantOverride(StringName.op_Implicit("shadow_offset_y"), 0);
				((Control)val).AddThemeConstantOverride(StringName.op_Implicit("shadow_outline_size"), 0);
			}
		}
		catch (Exception ex)
		{
			Log.Debug("[Danjin] StripHpLabelShadow 失败(可忽略):" + ex.Message, 2);
		}
	}

	private static void ApplyHpLabelOffset(Control label, float yShift)
	{
		LabelOffsetHolder value = _labelOffsets.GetValue(label, (Control _) => new LabelOffsetHolder());
		if (!value.Captured)
		{
			value.OrigOffsetTop = label.OffsetTop;
			value.OrigOffsetBottom = label.OffsetBottom;
			value.Captured = true;
		}
		label.OffsetTop = value.OrigOffsetTop + yShift;
		label.OffsetBottom = value.OrigOffsetBottom + yShift;
	}

	private static void HideVanillaVisuals(NHealthBar instance, Control hpForeground, Control hpFgContainer)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		((CanvasItem)hpForeground).SelfModulate = TransparentWhite;
		HideField(instance, "_hpMiddleground", visible: false);
		HideField(instance, "_poisonForeground", visible: true, selfModulate: true);
		HideField(instance, "_doomForeground", visible: true, selfModulate: true);
		HideField(instance, "_blockOutline", visible: false);
		HideField(instance, "_infinityTex", visible: false);
		BoxedBool value = _troughHidden.GetValue(instance, (NHealthBar _) => new BoxedBool());
		if (!value.Value)
		{
			HideUnknownVisualChildren(instance.HpBarContainer);
			HideUnknownVisualChildren(hpFgContainer);
			value.Value = true;
		}
	}

	private static void HideField(NHealthBar instance, string fieldName, bool visible = true, bool selfModulate = false)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			object? obj = AccessTools.Field(typeof(NHealthBar), fieldName)?.GetValue(instance);
			Control val = (Control)((obj is Control) ? obj : null);
			if (val != null)
			{
				if (selfModulate)
				{
					((CanvasItem)val).SelfModulate = TransparentWhite;
				}
				if (!visible)
				{
					((CanvasItem)val).Visible = false;
				}
			}
		}
		catch
		{
		}
	}

	private static void HideUnknownVisualChildren(Control? parent)
	{
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		if (parent == null)
		{
			return;
		}
		foreach (Node child in ((Node)parent).GetChildren(false))
		{
			Control val = (Control)(object)((child is Control) ? child : null);
			if (val == null || OurOverlayNames.Contains(((object)((Node)val).Name).ToString()))
			{
				continue;
			}
			switch (((object)((Node)val).Name).ToString())
			{
			case "HpForegroundContainer":
			case "HpForeground":
			case "HpMiddleground":
			case "PoisonForeground":
			case "DoomForeground":
			case "HpLabel":
			case "BlockContainer":
			case "BlockLabel":
			case "BlockOutline":
			case "InfinityTex":
				continue;
			}
			if ((val is TextureRect || val is NinePatchRect || val is ColorRect || val is Panel) ? true : false)
			{
				((CanvasItem)val).SelfModulate = TransparentWhite;
			}
		}
	}

	private static Texture2D? LoadTextureSafe(string resPath, ref Texture2D? cached)
	{
		if (cached != null)
		{
			return cached;
		}
		try
		{
			if (ResourceLoader.Exists(resPath, ""))
			{
				Texture2D val = ResourceLoader.Load<Texture2D>(resPath, (string)null, (CacheMode)1);
				if (val != null)
				{
					cached = val;
					return val;
				}
			}
		}
		catch (Exception ex)
		{
			Log.Debug("[Danjin] ResourceLoader 加载 " + resPath + " 失败: " + ex.Message, 2);
		}
		try
		{
			Image val2 = Image.LoadFromFile(resPath);
			if (val2 != null && !val2.IsEmpty())
			{
				ImageTexture val3 = ImageTexture.CreateFromImage(val2);
				if (val3 != null)
				{
					cached = (Texture2D?)(object)val3;
					return (Texture2D?)(object)val3;
				}
			}
		}
		catch (Exception ex2)
		{
			Log.Debug("[Danjin] Image.LoadFromFile 也失败：" + resPath + " - " + ex2.Message, 2);
		}
		return null;
	}
}
