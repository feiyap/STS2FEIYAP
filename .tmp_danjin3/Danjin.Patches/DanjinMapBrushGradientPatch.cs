using System;
using Danjin.Character;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Nodes.Screens.Map;

namespace Danjin.Patches;

[HarmonyPatch(typeof(NMapDrawings), "CreateLineForPlayer")]
public static class DanjinMapBrushGradientPatch
{
	private const string ShaderPath = "res://Danjin/Shaders/danjin_map_gradient.gdshader";

	private const float PERIOD = 320f;

	private const float Y_OFFSET = 0f;

	private static readonly Color COLOR_BLOOD = new Color(0.6f, 0.08f, 0.12f, 1f);

	private static readonly Color COLOR_ROUGE = new Color(0.6f, 0.08f, 0.12f, 1f);

	private static readonly Color COLOR_GLOW = new Color(1f, 0.2f, 0.3f, 1f);

	private static Shader? _cachedShader;

	private static bool _shaderLoadFailed;

	[HarmonyPostfix]
	public static void Postfix(Player player, bool isErasing, Line2D __result)
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Expected O, but got Unknown
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			if (!isErasing && ((player != null) ? player.Character : null) is Danjin.Character.Danjin)
			{
				Shader val = LoadShader();
				if (val != null)
				{
					ShaderMaterial val2 = new ShaderMaterial
					{
						Shader = val
					};
					val2.SetShaderParameter(StringName.op_Implicit("period"), Variant.op_Implicit(320f));
					val2.SetShaderParameter(StringName.op_Implicit("y_offset"), Variant.op_Implicit(0f));
					val2.SetShaderParameter(StringName.op_Implicit("color_blood"), Variant.op_Implicit(COLOR_BLOOD));
					val2.SetShaderParameter(StringName.op_Implicit("color_rouge"), Variant.op_Implicit(COLOR_ROUGE));
					val2.SetShaderParameter(StringName.op_Implicit("color_glow"), Variant.op_Implicit(COLOR_GLOW));
					((CanvasItem)__result).Material = (Material)(object)val2;
					__result.DefaultColor = Colors.White;
				}
			}
		}
		catch (Exception ex)
		{
			Log.Error("[Danjin] 地图画笔渐变 patch 失败:" + ex.Message, 2);
		}
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
			if (ResourceLoader.Exists("res://Danjin/Shaders/danjin_map_gradient.gdshader", ""))
			{
				_cachedShader = ResourceLoader.Load<Shader>("res://Danjin/Shaders/danjin_map_gradient.gdshader", (string)null, (CacheMode)1);
				if (_cachedShader != null)
				{
					return _cachedShader;
				}
			}
			Log.Error("[Danjin] 找不到 shader:res://Danjin/Shaders/danjin_map_gradient.gdshader", 2);
		}
		catch (Exception ex)
		{
			Log.Error("[Danjin] 加载 shader 失败:" + ex.Message, 2);
		}
		_shaderLoadFailed = true;
		return null;
	}
}
