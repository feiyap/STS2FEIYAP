using System;
using System.Collections.Generic;
using System.Reflection;
using Danjin.Character;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.addons.mega_text;

namespace Danjin.Patches;

[HarmonyPatch(typeof(NCard), "UpdateStarCostVisuals")]
[HarmonyPriority(0)]
internal static class DanjinEnergyIconGlowPatch
{
	private const string HaloTexturePath = "res://Danjin/Images/Energy Counters/danjin_orb_layer_halo.png";

	private const string HaloShaderPath = "res://Danjin/Shaders/danjin_jade_outer_halo.gdshader";

	private const string InnerBreathShaderPath = "res://Danjin/Shaders/danjin_jade_inner_breath.gdshader";

	private const string GlowNodeName = "DanjinEnergyHaloGlow";

	private const float StarExpandPx = 18f;

	private const float StarTranslateX = 0f;

	private const float StarTranslateY = -4f;

	private const float StarLabelOffsetY = 19f;

	private const string FireShaderPath = "res://Danjin/Shaders/danjin_stepped_fire.gdshader";

	private const string FireShapePath = "res://Danjin/Images/Vfx/necro_fire_shape.png";

	private const string TriNoisePath = "res://Danjin/Images/Vfx/triangle_noise_tile.png";

	private const string BasicNoisePath = "res://Danjin/Images/Vfx/basic_fire_noise.png";

	private const string ZigzagPath = "res://Danjin/Images/Vfx/zigzag_fire_distortion.png";

	private const string FireMaskPath = "res://Danjin/Images/Vfx/fire_bottom_mask.png";

	private const string NecroMaskPath = "res://Danjin/Images/Vfx/necro_fire_bottom_mask.png";

	private const float IconTranslateX = -6f;

	private const float IconTranslateY = -8f;

	private const float IconShrinkPx = 4f;

	private const float GlowExpandPx = 12f;

	private static readonly Color EnergyOutlineColor;

	private static readonly Color StarOutlineColor;

	private static readonly FieldInfo? _fEnergyIcon;

	private static readonly FieldInfo? _fUnplayableEnergyIcon;

	private static readonly FieldInfo? _fEnergyLabel;

	private static readonly FieldInfo? _fStarLabel;

	private static readonly FieldInfo? _fStarIcon;

	private static readonly FieldInfo? _fUnplayableStarIcon;

	private static readonly bool _fieldsOk;

	private static Texture2D? _cachedHaloTexture;

	private static Shader? _cachedHaloShader;

	private static Shader? _cachedInnerBreathShader;

	private static ShaderMaterial? _sharedInnerBreathMat;

	private static ShaderMaterial? _cachedFireMat;

	private static Texture2D? _cachedFireShape;

	private static readonly Dictionary<ulong, (float L, float T, float R, float B)> _originalIconOffsets;

	private static readonly Dictionary<ulong, Material?> _originalIconMaterials;

	private static readonly HashSet<ulong> _shiftedIcons;

	private static readonly HashSet<ulong> _innerBreathedIcons;

	private static readonly HashSet<ulong> _everOutlinedLabels;

	private static readonly Dictionary<ulong, Texture2D?> _originalStarTextures;

	private static readonly Dictionary<ulong, Material?> _originalStarMaterials;

	private static readonly Dictionary<ulong, (float L, float T, float R, float B)> _originalStarOffsets;

	private static readonly Dictionary<ulong, (ExpandModeEnum E, StretchModeEnum S)> _originalStarModes;

	private static readonly HashSet<ulong> _adjustedStars;

	private static readonly HashSet<ulong> _everOutlinedStarLabels;

	private static readonly Dictionary<ulong, (float T, float B)> _originalStarLabelOffsets;

	private static readonly HashSet<ulong> _shiftedStarLabels;

	static DanjinEnergyIconGlowPatch()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		EnergyOutlineColor = new Color(0.0235294f, 0.247059f, 0.290196f, 1f);
		StarOutlineColor = new Color(0.24f, 0f, 0.04f, 1f);
		_fEnergyIcon = AccessTools.Field(typeof(NCard), "_energyIcon");
		_fUnplayableEnergyIcon = AccessTools.Field(typeof(NCard), "_unplayableEnergyIcon");
		_fEnergyLabel = AccessTools.Field(typeof(NCard), "_energyLabel");
		_fStarLabel = AccessTools.Field(typeof(NCard), "_starLabel");
		_fStarIcon = AccessTools.Field(typeof(NCard), "_starIcon");
		_fUnplayableStarIcon = AccessTools.Field(typeof(NCard), "_unplayableStarIcon");
		_fieldsOk = _fEnergyIcon != null && _fUnplayableEnergyIcon != null && _fEnergyLabel != null;
		_originalIconOffsets = new Dictionary<ulong, (float, float, float, float)>();
		_originalIconMaterials = new Dictionary<ulong, Material>();
		_shiftedIcons = new HashSet<ulong>();
		_innerBreathedIcons = new HashSet<ulong>();
		_everOutlinedLabels = new HashSet<ulong>();
		_originalStarTextures = new Dictionary<ulong, Texture2D>();
		_originalStarMaterials = new Dictionary<ulong, Material>();
		_originalStarOffsets = new Dictionary<ulong, (float, float, float, float)>();
		_originalStarModes = new Dictionary<ulong, (ExpandModeEnum, StretchModeEnum)>();
		_adjustedStars = new HashSet<ulong>();
		_everOutlinedStarLabels = new HashSet<ulong>();
		_originalStarLabelOffsets = new Dictionary<ulong, (float, float)>();
		_shiftedStarLabels = new HashSet<ulong>();
		if (!_fieldsOk)
		{
			Log.Error(">>>[DanjinMod] EnergyIconGlow: NCard 字段定位失败,patch 不生效", 2);
			if (_fEnergyIcon == null)
			{
				Log.Error("  - 缺失 NCard._energyIcon", 2);
			}
			if (_fUnplayableEnergyIcon == null)
			{
				Log.Error("  - 缺失 NCard._unplayableEnergyIcon", 2);
			}
			if (_fEnergyLabel == null)
			{
				Log.Error("  - 缺失 NCard._energyLabel", 2);
			}
		}
		if (_fStarIcon == null)
		{
			Log.Warn(">>>[DanjinMod] EnergyIconGlow: NCard._starIcon 定位失败,星星火焰不生效", 2);
		}
	}

	[HarmonyPostfix]
	private static void Postfix(NCard __instance)
	{
		if (!_fieldsOk)
		{
			return;
		}
		try
		{
			CardModel model = __instance.Model;
			if (model != null)
			{
				bool isDanjin = model.Pool is DanjinCardPool;
				object? value = _fEnergyIcon.GetValue(__instance);
				TextureRect val = (TextureRect)((value is TextureRect) ? value : null);
				if (val != null)
				{
					HandleIcon(val, isDanjin);
				}
				object? value2 = _fUnplayableEnergyIcon.GetValue(__instance);
				TextureRect val2 = (TextureRect)((value2 is TextureRect) ? value2 : null);
				if (val2 != null)
				{
					HandleIcon(val2, isDanjin);
				}
				object? value3 = _fEnergyLabel.GetValue(__instance);
				MegaLabel val3 = (MegaLabel)((value3 is MegaLabel) ? value3 : null);
				if (val3 != null)
				{
					HandleLabel(val3, isDanjin);
				}
				object? obj = _fStarIcon?.GetValue(__instance);
				TextureRect val4 = (TextureRect)((obj is TextureRect) ? obj : null);
				if (val4 != null)
				{
					HandleStarIcon(val4, isDanjin);
				}
				object? obj2 = _fUnplayableStarIcon?.GetValue(__instance);
				TextureRect val5 = (TextureRect)((obj2 is TextureRect) ? obj2 : null);
				if (val5 != null)
				{
					HandleStarIcon(val5, isDanjin);
				}
				object? obj3 = _fStarLabel?.GetValue(__instance);
				MegaLabel val6 = (MegaLabel)((obj3 is MegaLabel) ? obj3 : null);
				if (val6 != null)
				{
					HandleStarLabel(val6, isDanjin);
				}
			}
		}
		catch (Exception value4)
		{
			Log.Error($">>>[DanjinMod] EnergyIcon patch 异常: {value4}", 2);
		}
	}

	private static void HandleIcon(TextureRect icon, bool isDanjin)
	{
		ulong instanceId = ((GodotObject)icon).GetInstanceId();
		if (!_originalIconOffsets.ContainsKey(instanceId))
		{
			_originalIconOffsets[instanceId] = (((Control)icon).OffsetLeft, ((Control)icon).OffsetTop, ((Control)icon).OffsetRight, ((Control)icon).OffsetBottom);
		}
		if (!_originalIconMaterials.ContainsKey(instanceId))
		{
			_originalIconMaterials[instanceId] = ((CanvasItem)icon).Material;
		}
		TextureRect nodeOrNull = ((Node)icon).GetNodeOrNull<TextureRect>(NodePath.op_Implicit("DanjinEnergyHaloGlow"));
		if (isDanjin)
		{
			if (nodeOrNull == null)
			{
				TextureRect val = CreateGlowLayer();
				if (val != null)
				{
					((Node)icon).AddChild((Node)(object)val, false, (InternalMode)0);
				}
			}
			else
			{
				((CanvasItem)nodeOrNull).Visible = true;
			}
			if (_shiftedIcons.Add(instanceId))
			{
				((Control)icon).OffsetLeft = ((Control)icon).OffsetLeft + -2f;
				((Control)icon).OffsetRight = ((Control)icon).OffsetRight + -10f;
				((Control)icon).OffsetTop = ((Control)icon).OffsetTop + -4f;
				((Control)icon).OffsetBottom = ((Control)icon).OffsetBottom + -12f;
			}
			if (_innerBreathedIcons.Add(instanceId))
			{
				ShaderMaterial sharedInnerBreathMaterial = GetSharedInnerBreathMaterial();
				if (sharedInnerBreathMaterial != null)
				{
					((CanvasItem)icon).Material = (Material)(object)sharedInnerBreathMaterial;
				}
			}
		}
		else
		{
			if (nodeOrNull != null)
			{
				((CanvasItem)nodeOrNull).Visible = false;
			}
			if (_shiftedIcons.Remove(instanceId) && _originalIconOffsets.TryGetValue(instanceId, out (float, float, float, float) value))
			{
				((Control)icon).OffsetLeft = value.Item1;
				((Control)icon).OffsetTop = value.Item2;
				((Control)icon).OffsetRight = value.Item3;
				((Control)icon).OffsetBottom = value.Item4;
			}
			if (_innerBreathedIcons.Remove(instanceId) && _originalIconMaterials.TryGetValue(instanceId, out Material value2))
			{
				((CanvasItem)icon).Material = value2;
			}
		}
	}

	private static void HandleStarIcon(TextureRect starIcon, bool isDanjin)
	{
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ef: Unknown result type (might be due to invalid IL or missing references)
		ulong instanceId = ((GodotObject)starIcon).GetInstanceId();
		if (!_originalStarTextures.ContainsKey(instanceId))
		{
			_originalStarTextures[instanceId] = starIcon.Texture;
		}
		if (!_originalStarMaterials.ContainsKey(instanceId))
		{
			_originalStarMaterials[instanceId] = ((CanvasItem)starIcon).Material;
		}
		if (!_originalStarOffsets.ContainsKey(instanceId))
		{
			_originalStarOffsets[instanceId] = (((Control)starIcon).OffsetLeft, ((Control)starIcon).OffsetTop, ((Control)starIcon).OffsetRight, ((Control)starIcon).OffsetBottom);
		}
		if (!_originalStarModes.ContainsKey(instanceId))
		{
			_originalStarModes[instanceId] = (starIcon.ExpandMode, starIcon.StretchMode);
		}
		if (isDanjin)
		{
			if (_cachedFireShape == null || !GodotObject.IsInstanceValid((GodotObject)(object)_cachedFireShape))
			{
				_cachedFireShape = GD.Load<Texture2D>("res://Danjin/Images/Vfx/necro_fire_shape.png");
			}
			if (_cachedFireShape != null)
			{
				starIcon.Texture = _cachedFireShape;
			}
			ShaderMaterial fireMaterial = GetFireMaterial();
			if (fireMaterial != null)
			{
				((CanvasItem)starIcon).Material = (Material)(object)fireMaterial;
			}
			starIcon.ExpandMode = (ExpandModeEnum)1;
			starIcon.StretchMode = (StretchModeEnum)5;
			if (_adjustedStars.Add(instanceId))
			{
				((Control)starIcon).OffsetLeft = ((Control)starIcon).OffsetLeft + -18f;
				((Control)starIcon).OffsetRight = ((Control)starIcon).OffsetRight + 18f;
				((Control)starIcon).OffsetTop = ((Control)starIcon).OffsetTop + -22f;
				((Control)starIcon).OffsetBottom = ((Control)starIcon).OffsetBottom + 14f;
			}
			return;
		}
		if (_originalStarTextures.TryGetValue(instanceId, out Texture2D value))
		{
			starIcon.Texture = value;
		}
		if (_originalStarMaterials.TryGetValue(instanceId, out Material value2))
		{
			((CanvasItem)starIcon).Material = value2;
		}
		if (_adjustedStars.Remove(instanceId))
		{
			if (_originalStarOffsets.TryGetValue(instanceId, out (float, float, float, float) value3))
			{
				((Control)starIcon).OffsetLeft = value3.Item1;
				((Control)starIcon).OffsetTop = value3.Item2;
				((Control)starIcon).OffsetRight = value3.Item3;
				((Control)starIcon).OffsetBottom = value3.Item4;
			}
			if (_originalStarModes.TryGetValue(instanceId, out (ExpandModeEnum, StretchModeEnum) value4))
			{
				starIcon.ExpandMode = value4.Item1;
				starIcon.StretchMode = value4.Item2;
			}
		}
	}

	private static void HandleLabel(MegaLabel label, bool isDanjin)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		ulong instanceId = ((GodotObject)label).GetInstanceId();
		if (isDanjin)
		{
			((Control)label).AddThemeColorOverride(StringName.op_Implicit("font_outline_color"), EnergyOutlineColor);
			_everOutlinedLabels.Add(instanceId);
		}
		else if (_everOutlinedLabels.Remove(instanceId))
		{
			((Control)label).RemoveThemeColorOverride(StringName.op_Implicit("font_outline_color"));
		}
	}

	private static void HandleStarLabel(MegaLabel label, bool isDanjin)
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		ulong instanceId = ((GodotObject)label).GetInstanceId();
		if (!_originalStarLabelOffsets.ContainsKey(instanceId))
		{
			_originalStarLabelOffsets[instanceId] = (((Control)label).OffsetTop, ((Control)label).OffsetBottom);
		}
		if (isDanjin)
		{
			((Control)label).AddThemeColorOverride(StringName.op_Implicit("font_outline_color"), StarOutlineColor);
			_everOutlinedStarLabels.Add(instanceId);
			if (_shiftedStarLabels.Add(instanceId))
			{
				((Control)label).OffsetTop = ((Control)label).OffsetTop + 19f;
				((Control)label).OffsetBottom = ((Control)label).OffsetBottom + 19f;
			}
			return;
		}
		if (_everOutlinedStarLabels.Remove(instanceId))
		{
			((Control)label).RemoveThemeColorOverride(StringName.op_Implicit("font_outline_color"));
		}
		if (_shiftedStarLabels.Remove(instanceId) && _originalStarLabelOffsets.TryGetValue(instanceId, out (float, float) value))
		{
			((Control)label).OffsetTop = value.Item1;
			((Control)label).OffsetBottom = value.Item2;
		}
	}

	private static TextureRect? CreateGlowLayer()
	{
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Expected O, but got Unknown
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Expected O, but got Unknown
		if (_cachedHaloTexture == null || !GodotObject.IsInstanceValid((GodotObject)(object)_cachedHaloTexture))
		{
			_cachedHaloTexture = GD.Load<Texture2D>("res://Danjin/Images/Energy Counters/danjin_orb_layer_halo.png");
		}
		if (_cachedHaloShader == null || !GodotObject.IsInstanceValid((GodotObject)(object)_cachedHaloShader))
		{
			_cachedHaloShader = GD.Load<Shader>("res://Danjin/Shaders/danjin_jade_outer_halo.gdshader");
		}
		if (_cachedHaloTexture == null)
		{
			Log.Error(">>>[DanjinMod] halo 贴图加载失败: res://Danjin/Images/Energy Counters/danjin_orb_layer_halo.png", 2);
			return null;
		}
		if (_cachedHaloShader == null)
		{
			Log.Error(">>>[DanjinMod] halo shader 加载失败: res://Danjin/Shaders/danjin_jade_outer_halo.gdshader", 2);
			return null;
		}
		ShaderMaterial val = new ShaderMaterial
		{
			Shader = _cachedHaloShader
		};
		val.SetShaderParameter(StringName.op_Implicit("breath_min"), Variant.op_Implicit(0.4f));
		val.SetShaderParameter(StringName.op_Implicit("breath_max"), Variant.op_Implicit(0.85f));
		val.SetShaderParameter(StringName.op_Implicit("breath_period"), Variant.op_Implicit(2.6f));
		val.SetShaderParameter(StringName.op_Implicit("energy_pulse"), Variant.op_Implicit(0f));
		val.SetShaderParameter(StringName.op_Implicit("dim_threshold"), Variant.op_Implicit(0.85f));
		TextureRect val2 = new TextureRect
		{
			Name = StringName.op_Implicit("DanjinEnergyHaloGlow"),
			Texture = _cachedHaloTexture,
			Material = (Material)(object)val,
			ShowBehindParent = true,
			MouseFilter = (MouseFilterEnum)2,
			StretchMode = (StretchModeEnum)5,
			ExpandMode = (ExpandModeEnum)1
		};
		((Control)val2).SetAnchorsPreset((LayoutPreset)15, false);
		((Control)val2).OffsetLeft = -12f;
		((Control)val2).OffsetTop = -12f;
		((Control)val2).OffsetRight = 12f;
		((Control)val2).OffsetBottom = 12f;
		return val2;
	}

	private static ShaderMaterial? GetSharedInnerBreathMaterial()
	{
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Expected O, but got Unknown
		if (_sharedInnerBreathMat != null && GodotObject.IsInstanceValid((GodotObject)(object)_sharedInnerBreathMat))
		{
			return _sharedInnerBreathMat;
		}
		if (_cachedInnerBreathShader == null || !GodotObject.IsInstanceValid((GodotObject)(object)_cachedInnerBreathShader))
		{
			_cachedInnerBreathShader = GD.Load<Shader>("res://Danjin/Shaders/danjin_jade_inner_breath.gdshader");
		}
		if (_cachedInnerBreathShader == null)
		{
			Log.Error(">>>[DanjinMod] inner_breath shader 加载失败: res://Danjin/Shaders/danjin_jade_inner_breath.gdshader", 2);
			return null;
		}
		ShaderMaterial val = new ShaderMaterial
		{
			Shader = _cachedInnerBreathShader
		};
		val.SetShaderParameter(StringName.op_Implicit("glow_color"), Variant.op_Implicit(new Color(0.55f, 0.92f, 1f, 1f)));
		val.SetShaderParameter(StringName.op_Implicit("breath_min"), Variant.op_Implicit(0.06f));
		val.SetShaderParameter(StringName.op_Implicit("breath_max"), Variant.op_Implicit(0.22f));
		val.SetShaderParameter(StringName.op_Implicit("breath_period"), Variant.op_Implicit(2.6f));
		val.SetShaderParameter(StringName.op_Implicit("glow_focus"), Variant.op_Implicit(0.35f));
		val.SetShaderParameter(StringName.op_Implicit("energy_pulse"), Variant.op_Implicit(0f));
		val.SetShaderParameter(StringName.op_Implicit("dim_threshold"), Variant.op_Implicit(0.85f));
		_sharedInnerBreathMat = val;
		return _sharedInnerBreathMat;
	}

	private static ShaderMaterial? GetFireMaterial()
	{
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		//IL_018a: Unknown result type (might be due to invalid IL or missing references)
		//IL_018f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0199: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01af: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01de: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_0202: Unknown result type (might be due to invalid IL or missing references)
		//IL_0207: Unknown result type (might be due to invalid IL or missing references)
		//IL_0211: Unknown result type (might be due to invalid IL or missing references)
		//IL_021d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0227: Unknown result type (might be due to invalid IL or missing references)
		//IL_0233: Unknown result type (might be due to invalid IL or missing references)
		//IL_023d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0252: Unknown result type (might be due to invalid IL or missing references)
		//IL_0257: Unknown result type (might be due to invalid IL or missing references)
		//IL_0261: Unknown result type (might be due to invalid IL or missing references)
		//IL_0276: Unknown result type (might be due to invalid IL or missing references)
		//IL_027b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0285: Unknown result type (might be due to invalid IL or missing references)
		//IL_0292: Unknown result type (might be due to invalid IL or missing references)
		//IL_029c: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02da: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_030a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0314: Unknown result type (might be due to invalid IL or missing references)
		//IL_0329: Unknown result type (might be due to invalid IL or missing references)
		//IL_032e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0338: Unknown result type (might be due to invalid IL or missing references)
		//IL_034d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0352: Unknown result type (might be due to invalid IL or missing references)
		//IL_035c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0368: Unknown result type (might be due to invalid IL or missing references)
		//IL_0372: Unknown result type (might be due to invalid IL or missing references)
		//IL_0382: Unknown result type (might be due to invalid IL or missing references)
		//IL_038c: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_03eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_03fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0405: Unknown result type (might be due to invalid IL or missing references)
		//IL_0415: Unknown result type (might be due to invalid IL or missing references)
		//IL_0424: Expected O, but got Unknown
		if (_cachedFireMat != null && GodotObject.IsInstanceValid((GodotObject)(object)_cachedFireMat))
		{
			return _cachedFireMat;
		}
		Shader val = GD.Load<Shader>("res://Danjin/Shaders/danjin_stepped_fire.gdshader");
		Texture2D val2 = GD.Load<Texture2D>("res://Danjin/Images/Vfx/triangle_noise_tile.png");
		Texture2D val3 = GD.Load<Texture2D>("res://Danjin/Images/Vfx/basic_fire_noise.png");
		Texture2D val4 = GD.Load<Texture2D>("res://Danjin/Images/Vfx/zigzag_fire_distortion.png");
		Texture2D val5 = GD.Load<Texture2D>("res://Danjin/Images/Vfx/fire_bottom_mask.png");
		Texture2D val6 = GD.Load<Texture2D>("res://Danjin/Images/Vfx/necro_fire_bottom_mask.png");
		if (val == null)
		{
			Log.Error(">>>[DanjinMod] 彤华火焰 shader 加载失败: res://Danjin/Shaders/danjin_stepped_fire.gdshader", 2);
			return null;
		}
		ShaderMaterial val7 = new ShaderMaterial
		{
			Shader = val
		};
		val7.SetShaderParameter(StringName.op_Implicit("GlobalOffset"), Variant.op_Implicit(Vector2.Zero));
		val7.SetShaderParameter(StringName.op_Implicit("OuterColor"), Variant.op_Implicit(new Color(0.82f, 0.12f, 0.42f, 1f)));
		val7.SetShaderParameter(StringName.op_Implicit("InnerColor"), Variant.op_Implicit(new Color(0.32f, 0.04f, 0.16f, 1f)));
		val7.SetShaderParameter(StringName.op_Implicit("InnerColorStep"), Variant.op_Implicit(new Vector2(0.52f, 0.92f)));
		val7.SetShaderParameter(StringName.op_Implicit("OuterStep"), Variant.op_Implicit(new Vector2(0.025f, 0.045f)));
		val7.SetShaderParameter(StringName.op_Implicit("Noise2Strength"), Variant.op_Implicit(0.51f));
		val7.SetShaderParameter(StringName.op_Implicit("Noise2Scaling"), Variant.op_Implicit(new Vector2(3f, 3f)));
		val7.SetShaderParameter(StringName.op_Implicit("Noise2Panning"), Variant.op_Implicit(new Vector2(-0.2f, 1.4f)));
		val7.SetShaderParameter(StringName.op_Implicit("Noise2Texture"), Variant.op_Implicit((GodotObject)(object)val3));
		val7.SetShaderParameter(StringName.op_Implicit("Noise1Strength"), Variant.op_Implicit(0.87f));
		val7.SetShaderParameter(StringName.op_Implicit("Noise1Scaling"), Variant.op_Implicit(new Vector2(2.2f, 2.2f)));
		val7.SetShaderParameter(StringName.op_Implicit("Noise1Panning"), Variant.op_Implicit(new Vector2(0.8f, 1.3f)));
		val7.SetShaderParameter(StringName.op_Implicit("Noise1Texture"), Variant.op_Implicit((GodotObject)(object)val2));
		val7.SetShaderParameter(StringName.op_Implicit("InvertNoiseMask"), Variant.op_Implicit(true));
		val7.SetShaderParameter(StringName.op_Implicit("NoiseMaskScale"), Variant.op_Implicit(new Vector2(1f, 0.91f)));
		val7.SetShaderParameter(StringName.op_Implicit("NoiseMaskOffset"), Variant.op_Implicit(new Vector2(0f, -0.015f)));
		val7.SetShaderParameter(StringName.op_Implicit("NoiseMask"), Variant.op_Implicit((GodotObject)(object)val5));
		val7.SetShaderParameter(StringName.op_Implicit("Distortion2Scale"), Variant.op_Implicit(new Vector2(1f, 0.8f)));
		val7.SetShaderParameter(StringName.op_Implicit("Distortion2Panning"), Variant.op_Implicit(new Vector2(0f, 0.5f)));
		val7.SetShaderParameter(StringName.op_Implicit("Distortion2Texture"), Variant.op_Implicit((GodotObject)(object)val4));
		val7.SetShaderParameter(StringName.op_Implicit("Distortion2Strength"), Variant.op_Implicit(0.15f));
		val7.SetShaderParameter(StringName.op_Implicit("Distortion1Scale"), Variant.op_Implicit(new Vector2(1f, 1f)));
		val7.SetShaderParameter(StringName.op_Implicit("Distortion1Panning"), Variant.op_Implicit(new Vector2(0.03f, 0.6f)));
		val7.SetShaderParameter(StringName.op_Implicit("Distortion1Texture"), Variant.op_Implicit((GodotObject)(object)val2));
		val7.SetShaderParameter(StringName.op_Implicit("Distortion1Strength"), Variant.op_Implicit(0.125f));
		val7.SetShaderParameter(StringName.op_Implicit("DistortionMaskScale"), Variant.op_Implicit(new Vector2(1f, 0.88f)));
		val7.SetShaderParameter(StringName.op_Implicit("DistortionMaskOffset"), Variant.op_Implicit(new Vector2(0f, 0.125f)));
		val7.SetShaderParameter(StringName.op_Implicit("DistortionMask"), Variant.op_Implicit((GodotObject)(object)val6));
		val7.SetShaderParameter(StringName.op_Implicit("s"), Variant.op_Implicit(1f));
		val7.SetShaderParameter(StringName.op_Implicit("v"), Variant.op_Implicit(1f));
		_cachedFireMat = val7;
		return _cachedFireMat;
	}
}
