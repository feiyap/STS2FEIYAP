using System;
using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Nodes.Screens.Map;

namespace Danjin.Patches;

[HarmonyPatch(typeof(NMapDrawings))]
public static class DanjinEraserAlphaOnlyPatch
{
	private const string ShaderPath = "res://Danjin/Shaders/danjin_eraser_alpha_only.gdshader";

	private static readonly FieldInfo? _eraserMaterialField = AccessTools.Field(typeof(NMapDrawings), "_eraserMaterial");

	private static ShaderMaterial? _alphaOnlyEraserMaterial;

	private static Shader? _cachedShader;

	private static bool _shaderLoadFailed;

	[HarmonyPatch("_Ready")]
	[HarmonyPostfix]
	public static void ReadyPostfix(NMapDrawings __instance)
	{
		try
		{
			if (_eraserMaterialField == null)
			{
				Log.Error("[Danjin] 找不到 NMapDrawings._eraserMaterial 字段,橡皮擦修复跳过", 2);
				return;
			}
			ShaderMaterial orCreateAlphaOnlyMaterial = GetOrCreateAlphaOnlyMaterial();
			if (orCreateAlphaOnlyMaterial != null)
			{
				_eraserMaterialField.SetValue(__instance, orCreateAlphaOnlyMaterial);
			}
		}
		catch (Exception ex)
		{
			Log.Error("[Danjin] 橡皮擦 _Ready patch 失败:" + ex.Message, 2);
		}
	}

	[HarmonyPatch("CreateLineForPlayer")]
	[HarmonyPostfix]
	[HarmonyPriority(200)]
	public static void CreateLineForPlayerPostfix(NMapDrawings __instance, Player player, bool isErasing, Line2D __result)
	{
		try
		{
			if (isErasing && __result != null && !(_eraserMaterialField == null))
			{
				object? value = _eraserMaterialField.GetValue(__instance);
				Material val = (Material)((value is Material) ? value : null);
				if (val != null)
				{
					((CanvasItem)__result).Material = val;
				}
			}
		}
		catch (Exception ex)
		{
			Log.Error("[Danjin] 橡皮擦 CreateLineForPlayer patch 失败:" + ex.Message, 2);
		}
	}

	private static ShaderMaterial? GetOrCreateAlphaOnlyMaterial()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Expected O, but got Unknown
		if (_alphaOnlyEraserMaterial != null)
		{
			return _alphaOnlyEraserMaterial;
		}
		Shader val = LoadShader();
		if (val == null)
		{
			return null;
		}
		_alphaOnlyEraserMaterial = new ShaderMaterial
		{
			Shader = val
		};
		return _alphaOnlyEraserMaterial;
	}

	private static Shader? LoadShader()
	{
		if (_cachedShader != null)
		{
			return _cachedShader;
		}
		if (_shaderLoadFailed)
		{
			return null;
		}
		try
		{
			if (ResourceLoader.Exists("res://Danjin/Shaders/danjin_eraser_alpha_only.gdshader", ""))
			{
				_cachedShader = ResourceLoader.Load<Shader>("res://Danjin/Shaders/danjin_eraser_alpha_only.gdshader", (string)null, (CacheMode)1);
				if (_cachedShader != null)
				{
					return _cachedShader;
				}
			}
			Log.Error("[Danjin] 找不到橡皮擦 shader:res://Danjin/Shaders/danjin_eraser_alpha_only.gdshader", 2);
		}
		catch (Exception ex)
		{
			Log.Error("[Danjin] 加载橡皮擦 shader 失败:" + ex.Message, 2);
		}
		_shaderLoadFailed = true;
		return null;
	}
}
