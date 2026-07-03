using System;
using Danjin.Cards;
using Godot;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;

namespace Danjin.Patches;

internal static class DanjinAncientLook
{
	private const string BannerShaderPath = "res://Danjin/Shaders/danjin_banner_recolor.gdshader";

	private const float BannerHue = 0.94f;

	private const float BannerSat = 0.85f;

	private const float BannerValMin = 0.45f;

	private const float BannerValMax = 0.85f;

	private const float PlaqueHue = 0.94f;

	private const float PlaqueSat = 0.55f;

	private const float PlaqueValMin = 0.7f;

	private const float PlaqueValMax = 1f;

	private const float FlameHue = 0.96f;

	private const float FlameSat = 0.9f;

	private const float FlameValMin = 0.55f;

	private const float FlameValMax = 1f;

	private static readonly StringName P_Hue = new StringName("target_hue");

	private static readonly StringName P_Sat = new StringName("target_sat");

	private static readonly StringName P_VMin = new StringName("val_min");

	private static readonly StringName P_VMax = new StringName("val_max");

	private static readonly StringName P_PreserveWhites = new StringName("preserve_whites");

	private static Shader? _shader;

	private static bool _shaderFailed;

	private static ShaderMaterial? _bannerMat;

	private static ShaderMaterial? _plaqueMat;

	private static ShaderMaterial? _flameMat;

	public static bool IsDanjinCard(CardModel model)
	{
		if (!(model is DanjinCard))
		{
			return model is DanjinTokenCard;
		}
		return true;
	}

	private static Shader? GetShader()
	{
		if (_shader != null)
		{
			return _shader;
		}
		if (_shaderFailed)
		{
			return null;
		}
		try
		{
			if (ResourceLoader.Exists("res://Danjin/Shaders/danjin_banner_recolor.gdshader", ""))
			{
				_shader = ResourceLoader.Load<Shader>("res://Danjin/Shaders/danjin_banner_recolor.gdshader", (string)null, (CacheMode)1);
				if (_shader != null)
				{
					return _shader;
				}
			}
			Log.Error("[Danjin] 找不到 banner shader: res://Danjin/Shaders/danjin_banner_recolor.gdshader", 2);
		}
		catch (Exception ex)
		{
			Log.Error("[Danjin] 加载 banner shader 失败: " + ex.Message, 2);
		}
		_shaderFailed = true;
		return null;
	}

	private static ShaderMaterial? MakeMat(ref ShaderMaterial? cache, float h, float s, float vmin, float vmax, bool preserveWhites = false)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Expected O, but got Unknown
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		if (cache != null)
		{
			return cache;
		}
		Shader shader = GetShader();
		if (shader == null)
		{
			return null;
		}
		ShaderMaterial val = new ShaderMaterial
		{
			Shader = shader
		};
		val.SetShaderParameter(P_Hue, Variant.op_Implicit(h));
		val.SetShaderParameter(P_Sat, Variant.op_Implicit(s));
		val.SetShaderParameter(P_VMin, Variant.op_Implicit(vmin));
		val.SetShaderParameter(P_VMax, Variant.op_Implicit(vmax));
		val.SetShaderParameter(P_PreserveWhites, Variant.op_Implicit(preserveWhites));
		cache = val;
		return cache;
	}

	public static ShaderMaterial? BannerMat()
	{
		return MakeMat(ref _bannerMat, 0.94f, 0.85f, 0.45f, 0.85f);
	}

	public static ShaderMaterial? PlaqueMat()
	{
		return MakeMat(ref _plaqueMat, 0.94f, 0.55f, 0.7f, 1f);
	}

	public static ShaderMaterial? FlameMat()
	{
		return MakeMat(ref _flameMat, 0.96f, 0.9f, 0.55f, 1f, preserveWhites: true);
	}
}
