using Godot;
using MegaCrit.Sts2.Core.Nodes.Vfx;

namespace Danjin.Patches;

internal static class DanjinTrailRecolor
{
	public static void Apply(NCardTrailVfx trail)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Expected O, but got Unknown
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Expected O, but got Unknown
		//IL_0180: Unknown result type (might be due to invalid IL or missing references)
		//IL_0185: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0218: Unknown result type (might be due to invalid IL or missing references)
		Line2D nodeOrNull = ((Node)trail).GetNodeOrNull<Line2D>(NodePath.op_Implicit("Trails/OuterTrail"));
		if (nodeOrNull != null)
		{
			((CanvasItem)nodeOrNull).Modulate = new Color(0.85f, 0.07f, 0.25f, 0.752941f);
		}
		Line2D nodeOrNull2 = ((Node)trail).GetNodeOrNull<Line2D>(NodePath.op_Implicit("Trails/InnerTrail"));
		if (nodeOrNull2 != null)
		{
			((CanvasItem)nodeOrNull2).Modulate = new Color(1f, 0.45f, 0.55f, 0.501961f);
		}
		CpuParticles2D nodeOrNull3 = ((Node)trail).GetNodeOrNull<CpuParticles2D>(NodePath.op_Implicit("Sprites/BigSparks"));
		if (nodeOrNull3 != null)
		{
			Gradient val = new Gradient();
			val.Offsets = new float[4] { 0f, 0.554377f, 0.806366f, 1f };
			val.Colors = (Color[])(object)new Color[4]
			{
				new Color(0.85f, 0.07f, 0.24f, 1f),
				new Color(0.55f, 0.1f, 0.2f, 0.447059f),
				new Color(0.4f, 0.05f, 0.15f, 0.482353f),
				new Color(0.4f, 0.1f, 0.18f, 0f)
			};
			nodeOrNull3.ColorRamp = val;
		}
		CpuParticles2D nodeOrNull4 = ((Node)trail).GetNodeOrNull<CpuParticles2D>(NodePath.op_Implicit("Sprites/LittleSparks"));
		if (nodeOrNull4 != null)
		{
			Gradient val = new Gradient();
			val.Offsets = new float[2] { 0f, 1f };
			val.Colors = (Color[])(object)new Color[2]
			{
				new Color(1f, 0.92f, 0.75f, 1f),
				new Color(0.95f, 0.5f, 0.25f, 0f)
			};
			nodeOrNull4.ColorRamp = val;
		}
		Sprite2D nodeOrNull5 = ((Node)trail).GetNodeOrNull<Sprite2D>(NodePath.op_Implicit("Sprites/Sprite2D2"));
		if (nodeOrNull5 != null)
		{
			((CanvasItem)nodeOrNull5).Modulate = new Color(0.65f, 0.04f, 0.2f, 0.752941f);
		}
		Sprite2D nodeOrNull6 = ((Node)trail).GetNodeOrNull<Sprite2D>(NodePath.op_Implicit("Sprites/Sprite2D3"));
		if (nodeOrNull6 != null)
		{
			((CanvasItem)nodeOrNull6).Modulate = new Color(1f, 0.4f, 0.55f, 0.501961f);
		}
	}
}
