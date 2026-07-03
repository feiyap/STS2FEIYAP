using System;
using Godot;

namespace Danjin.Character;

public static class DanjinFrameMaterial
{
	private const string HsvShaderPath = "res://shaders/hsv.gdshader";

	private static ShaderMaterial? _danjinTint;

	public static ShaderMaterial DanjinTint()
	{
		if (_danjinTint == null)
		{
			_danjinTint = CreateHsvMaterial(0.95f, 0.92f, 0.92f);
		}
		return _danjinTint;
	}

	private static ShaderMaterial CreateHsvMaterial(float h, float s, float v)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Expected O, but got Unknown
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Expected O, but got Unknown
		Shader val = GD.Load<Shader>("res://shaders/hsv.gdshader");
		if (val == null)
		{
			throw new InvalidOperationException("Failed to load HSV shader at 'res://shaders/hsv.gdshader'. 游戏内置 shader 不应缺失。");
		}
		ShaderMaterial val2 = new ShaderMaterial
		{
			Shader = (Shader)((Resource)val).Duplicate(false)
		};
		val2.SetShaderParameter(StringName.op_Implicit("h"), Variant.op_Implicit(h));
		val2.SetShaderParameter(StringName.op_Implicit("s"), Variant.op_Implicit(s));
		val2.SetShaderParameter(StringName.op_Implicit("v"), Variant.op_Implicit(v));
		return val2;
	}
}
