using System;
using System.Reflection;
using Danjin.Character;
using Danjin.Extensions;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.addons.mega_text;

namespace Danjin.Patches;

internal static class TonghuaCounterSpawnPatch
{
	[HarmonyPatch(typeof(NCombatUi), "Activate")]
	private static class ActivatePostfix
	{
		[HarmonyPostfix]
		private static void Postfix(NCombatUi __instance, CombatState state)
		{
			try
			{
				Player me = LocalContext.GetMe((ICombatState)(object)state);
				object? obj = F_starCounter?.GetValue(__instance);
				NStarCounter val = (NStarCounter)((obj is NStarCounter) ? obj : null);
				if (val == null)
				{
					Log.Error("[Danjin] 取不到原版 NStarCounter,跳过彤华 counter 创建", 2);
				}
				else
				{
					SpawnTonghuaCounter(val, me);
				}
			}
			catch (Exception value)
			{
				Log.Error($"[Danjin] TonghuaCounterSpawnPatch.Postfix 异常: {value}", 2);
			}
		}
	}

	private static readonly FieldInfo? F_starCounter = AccessTools.Field(typeof(NCombatUi), "_starCounter");

	private static readonly FieldInfo? F_player = AccessTools.Field(typeof(NStarCounter), "_player");

	private static readonly FieldInfo? F_label = AccessTools.Field(typeof(NStarCounter), "_label");

	private static readonly FieldInfo? F_rotationLayers = AccessTools.Field(typeof(NStarCounter), "_rotationLayers");

	private static readonly FieldInfo? F_icon = AccessTools.Field(typeof(NStarCounter), "_icon");

	private static readonly FieldInfo? F_hsv = AccessTools.Field(typeof(NStarCounter), "_hsv");

	private static readonly FieldInfo? F_hoverTip = AccessTools.Field(typeof(NStarCounter), "_hoverTip");

	private static readonly MethodInfo? M_ConnectStarsChangedSignal = AccessTools.Method(typeof(NStarCounter), "ConnectStarsChangedSignal", (Type[])null, (Type[])null);

	private static readonly MethodInfo? M_RefreshVisibility = AccessTools.Method(typeof(NStarCounter), "RefreshVisibility", (Type[])null, (Type[])null);

	private static readonly Vector2 DefaultPositionOffset = new Vector2(95f, -20f);

	private const float TonghuaScaleMultiplier = 1.4f;

	private const string TonghuaScenePath = "res://Danjin/Scenes/danjin_tonghua_counter.tscn";

	private const string FallbackIconPath = "res://Danjin/Images/Charui/tonghua_text_icon.png";

	private const string HoverTipSheet = "static_hover_tips";

	private const string TonghuaTitleKey = "TONGHUA_COUNT.title";

	private const string TonghuaDescKey = "TONGHUA_COUNT.description";

	private static string SingleTonghuaIconBBCode => "[img]" + "Charui/tonghua_text_icon.png".ImagePath() + "[/img]";

	private static PackedScene? LoadTonghuaSceneRobust()
	{
		PackedScene val = GD.Load<PackedScene>("res://Danjin/Scenes/danjin_tonghua_counter.tscn");
		if (val != null)
		{
			return val;
		}
		try
		{
			val = ResourceLoader.Load<PackedScene>("res://Danjin/Scenes/danjin_tonghua_counter.tscn", "PackedScene", (CacheMode)0);
			if (val != null)
			{
				Log.Info("[Danjin] 第二次重试加载彤华场景成功(CacheMode.Ignore)", 2);
				return val;
			}
		}
		catch (Exception ex)
		{
			Log.Warn("[Danjin] ResourceLoader 重试加载彤华场景异常: " + ex.Message, 2);
		}
		return null;
	}

	private static void SpawnTonghuaCounter(NStarCounter original, Player me)
	{
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		Node obj = ((Node)original).Duplicate(15);
		NStarCounter val = (NStarCounter)(object)((obj is NStarCounter) ? obj : null);
		if (val == null)
		{
			Log.Error("[Danjin] Duplicate NStarCounter 失败", 2);
			return;
		}
		Node parent = ((Node)original).GetParent();
		if (parent == null)
		{
			Log.Error("[Danjin] 原版 _starCounter 没有父节点,跳过", 2);
			((Node)val).QueueFree();
			return;
		}
		parent.AddChild((Node)(object)val, false, (InternalMode)0);
		EnsureFieldsInjected(val);
		TonghuaUiBindingPatch.MarkAsTonghuaCounter(val);
		F_player?.SetValue(val, me);
		try
		{
			M_ConnectStarsChangedSignal?.Invoke(val, null);
		}
		catch (Exception value)
		{
			Log.Error($"[Danjin] 调 ConnectStarsChangedSignal 失败: {value}", 2);
		}
		ApplyTonghuaVisuals(val);
		ReplaceHoverTipWithTonghua(val);
		((Control)val).Position = ((Control)original).Position + DefaultPositionOffset;
		((Control)val).Scale = ((Control)original).Scale * 1.4f;
		((CanvasItem)val).Visible = me.Character is Danjin.Character.Danjin;
		try
		{
			M_RefreshVisibility?.Invoke(val, null);
		}
		catch (Exception value2)
		{
			Log.Error($"[Danjin] 调 RefreshVisibility 失败: {value2}", 2);
		}
		try
		{
			ResourceLoader.Load<PackedScene>(NFireBurstVfx.scenePath, (string)null, (CacheMode)1);
		}
		catch (Exception ex)
		{
			Log.Warn("[Danjin] 预热迸发特效失败: " + ex.Message, 2);
		}
		Log.Info(">>>[DanjinMod] 彤华 counter 已创建并挂载", 2);
	}

	private static void ReplaceHoverTipWithTonghua(NStarCounter dup)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Expected O, but got Unknown
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Expected O, but got Unknown
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		if (F_hoverTip == null)
		{
			return;
		}
		try
		{
			LocString val = new LocString("static_hover_tips", "TONGHUA_COUNT.description");
			val.Add("singleTonghuaIcon", SingleTonghuaIconBBCode);
			LocString val2 = new LocString("static_hover_tips", "TONGHUA_COUNT.title");
			HoverTip val3 = default(HoverTip);
			((HoverTip)(ref val3))._002Ector(val2, val, (Texture2D)null);
			F_hoverTip.SetValue(dup, val3);
		}
		catch (Exception value)
		{
			Log.Error($"[Danjin] 替换彤华 hover tip 失败(显示 Stars 描述): {value}", 2);
		}
	}

	private static void EnsureFieldsInjected(NStarCounter dup)
	{
		if (F_label != null && F_label.GetValue(dup) == null)
		{
			Node obj = ((Node)dup).FindChild("CountLabel", true, false);
			MegaRichTextLabel val = (MegaRichTextLabel)(object)((obj is MegaRichTextLabel) ? obj : null);
			if (val != null)
			{
				F_label.SetValue(dup, val);
			}
			else
			{
				Log.Error("[Danjin] 找不到 CountLabel 节点(MegaRichTextLabel),数字将无法显示", 2);
			}
		}
		if (F_rotationLayers != null && F_rotationLayers.GetValue(dup) == null)
		{
			Node obj2 = ((Node)dup).FindChild("RotationLayers", true, false);
			Control val2 = (Control)(object)((obj2 is Control) ? obj2 : null);
			if (val2 != null)
			{
				F_rotationLayers.SetValue(dup, val2);
			}
		}
		if (!(F_icon != null) || F_icon.GetValue(dup) != null)
		{
			return;
		}
		Control nodeOrNull = ((Node)dup).GetNodeOrNull<Control>(NodePath.op_Implicit("Icon"));
		if (nodeOrNull != null)
		{
			F_icon.SetValue(dup, nodeOrNull);
			Material material = ((CanvasItem)nodeOrNull).Material;
			ShaderMaterial val3 = (ShaderMaterial)(object)((material is ShaderMaterial) ? material : null);
			if (val3 != null)
			{
				F_hsv?.SetValue(dup, val3);
			}
		}
	}

	private static void ApplyTonghuaVisuals(NStarCounter dup)
	{
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Expected O, but got Unknown
		PackedScene val = LoadTonghuaSceneRobust();
		if (val == null)
		{
			Log.Error("[Danjin] 加载彤华 counter 场景失败(已重试), 启用 fallback 避免显示成辉星: res://Danjin/Scenes/danjin_tonghua_counter.tscn", 2);
			ApplyTonghuaVisualsFallback(dup);
			return;
		}
		Control val2 = val.Instantiate<Control>((GenEditState)0);
		try
		{
			TextureRect nodeOrNull = ((Node)val2).GetNodeOrNull<TextureRect>(NodePath.op_Implicit("Icon"));
			Control nodeOrNull2 = ((Node)dup).GetNodeOrNull<Control>(NodePath.op_Implicit("Icon"));
			if (nodeOrNull != null && nodeOrNull.Texture != null)
			{
				TextureRect val3 = (TextureRect)(object)((nodeOrNull2 is TextureRect) ? nodeOrNull2 : null);
				if (val3 != null)
				{
					val3.Texture = nodeOrNull.Texture;
					Material material = ((CanvasItem)nodeOrNull).Material;
					ShaderMaterial val4 = (ShaderMaterial)(object)((material is ShaderMaterial) ? material : null);
					if (val4 != null)
					{
						ShaderMaterial value = (ShaderMaterial)(object)(((CanvasItem)val3).Material = (Material)(ShaderMaterial)((Resource)val4).Duplicate(true));
						F_hsv?.SetValue(dup, value);
					}
				}
				else if (nodeOrNull2 != null)
				{
					Node parent = ((Node)nodeOrNull2).GetParent();
					int index = ((Node)nodeOrNull2).GetIndex(false);
					Node obj = ((Node)nodeOrNull).Duplicate(15);
					TextureRect val6 = (TextureRect)(object)((obj is TextureRect) ? obj : null);
					if (val6 != null)
					{
						parent.RemoveChild((Node)(object)nodeOrNull2);
						((Node)nodeOrNull2).QueueFree();
						parent.AddChild((Node)(object)val6, false, (InternalMode)0);
						parent.MoveChild((Node)(object)val6, index);
						F_icon?.SetValue(dup, val6);
						F_hsv?.SetValue(dup, (object?)/*isinst with value type is only supported in some contexts*/);
					}
				}
			}
			object? obj2 = F_rotationLayers?.GetValue(dup);
			Control val7 = (Control)((obj2 is Control) ? obj2 : null);
			if (val7 != null)
			{
				((CanvasItem)val7).Visible = false;
			}
			MarginContainer nodeOrNull3 = ((Node)dup).GetNodeOrNull<MarginContainer>(NodePath.op_Implicit("MarginContainer"));
			MarginContainer nodeOrNull4 = ((Node)val2).GetNodeOrNull<MarginContainer>(NodePath.op_Implicit("MarginContainer"));
			if (nodeOrNull3 != null && nodeOrNull4 != null)
			{
				CopyMarginConstants(nodeOrNull4, nodeOrNull3);
			}
			Node obj3 = ((Node)val2).FindChild("CountLabel", true, false);
			RichTextLabel val8 = (RichTextLabel)(object)((obj3 is RichTextLabel) ? obj3 : null);
			Node obj4 = ((Node)dup).FindChild("CountLabel", true, false);
			RichTextLabel val9 = (RichTextLabel)(object)((obj4 is RichTextLabel) ? obj4 : null);
			if (val8 != null && val9 != null)
			{
				CopyLabelTheme(val8, val9);
			}
		}
		catch (Exception value2)
		{
			Log.Error($"[Danjin] 彤华视觉换装异常: {value2}", 2);
		}
		finally
		{
			((Node)val2).QueueFree();
		}
	}

	private static void ApplyTonghuaVisualsFallback(NStarCounter dup)
	{
		try
		{
			Texture2D val = ResourceLoader.Load<Texture2D>("res://Danjin/Images/Charui/tonghua_text_icon.png", (string)null, (CacheMode)1);
			if (val != null)
			{
				Control nodeOrNull = ((Node)dup).GetNodeOrNull<Control>(NodePath.op_Implicit("Icon"));
				TextureRect val2 = (TextureRect)(object)((nodeOrNull is TextureRect) ? nodeOrNull : null);
				if (val2 != null)
				{
					val2.Texture = val;
					((CanvasItem)val2).Material = null;
					F_hsv?.SetValue(dup, null);
					Log.Warn("[Danjin] Fallback: 已用 tonghua_text_icon.png 替换默认 Icon(无火焰 shader 但避免显示成辉星)", 2);
					goto IL_0065;
				}
			}
			if (val == null)
			{
				Log.Error("[Danjin] Fallback PNG 加载失败: res://Danjin/Images/Charui/tonghua_text_icon.png", 2);
			}
			goto IL_0065;
			IL_0065:
			object? obj = F_rotationLayers?.GetValue(dup);
			Control val3 = (Control)((obj is Control) ? obj : null);
			if (val3 != null)
			{
				((CanvasItem)val3).Visible = false;
			}
		}
		catch (Exception value)
		{
			Log.Error($"[Danjin] Fallback Icon 替换也失败, 隐藏彤华 counter 避免错误视觉: {value}", 2);
			((CanvasItem)dup).Visible = false;
		}
	}

	private static void CopyLabelTheme(RichTextLabel src, RichTextLabel dst)
	{
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		string[] array = new string[4] { "font_outline_color", "default_color", "font_shadow_color", "font_color" };
		foreach (string text in array)
		{
			try
			{
				if (((Control)src).HasThemeColorOverride(StringName.op_Implicit(text)))
				{
					((Control)dst).AddThemeColorOverride(StringName.op_Implicit(text), ((Control)src).GetThemeColor(StringName.op_Implicit(text), (StringName)null));
				}
			}
			catch
			{
			}
		}
		array = new string[4] { "outline_size", "shadow_offset_x", "shadow_offset_y", "shadow_outline_size" };
		foreach (string text2 in array)
		{
			try
			{
				if (((Control)src).HasThemeConstantOverride(StringName.op_Implicit(text2)))
				{
					((Control)dst).AddThemeConstantOverride(StringName.op_Implicit(text2), ((Control)src).GetThemeConstant(StringName.op_Implicit(text2), (StringName)null));
				}
			}
			catch
			{
			}
		}
		array = new string[5] { "normal_font_size", "bold_font_size", "bold_italics_font_size", "italics_font_size", "mono_font_size" };
		foreach (string text3 in array)
		{
			try
			{
				if (((Control)src).HasThemeFontSizeOverride(StringName.op_Implicit(text3)))
				{
					((Control)dst).AddThemeFontSizeOverride(StringName.op_Implicit(text3), ((Control)src).GetThemeFontSize(StringName.op_Implicit(text3), (StringName)null));
				}
			}
			catch
			{
			}
		}
	}

	private static void CopyMarginConstants(MarginContainer src, MarginContainer dst)
	{
		((Control)dst).OffsetTop = ((Control)src).OffsetTop;
		((Control)dst).OffsetBottom = ((Control)src).OffsetBottom;
		((Control)dst).OffsetLeft = ((Control)src).OffsetLeft;
		((Control)dst).OffsetRight = ((Control)src).OffsetRight;
		TryCopyConstant(src, dst, "margin_top");
		TryCopyConstant(src, dst, "margin_bottom");
		TryCopyConstant(src, dst, "margin_left");
		TryCopyConstant(src, dst, "margin_right");
	}

	private static void TryCopyConstant(MarginContainer src, MarginContainer dst, string name)
	{
		try
		{
			if (((Control)src).HasThemeConstantOverride(StringName.op_Implicit(name)) || ((Control)src).HasThemeConstant(StringName.op_Implicit(name), (StringName)null))
			{
				((Control)dst).AddThemeConstantOverride(StringName.op_Implicit(name), ((Control)src).GetThemeConstant(StringName.op_Implicit(name), (StringName)null));
			}
		}
		catch
		{
		}
	}
}
