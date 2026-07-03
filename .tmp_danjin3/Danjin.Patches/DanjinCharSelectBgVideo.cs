using System;
using Danjin.Utils;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect;

namespace Danjin.Patches;

public static class DanjinCharSelectBgVideo
{
	private const string LetterboxNodeName = "DanjinBgLetterbox";

	private const string VideoNodeName = "DanjinBgVideo";

	private const string TakenOverMeta = "_DanjinBgVideoTakenOver";

	private const float VideoAspect = 1.7777778f;

	private static readonly FieldRef<NCharacterSelectScreen, Control> _bgContainerRef = AccessTools.FieldRefAccess<NCharacterSelectScreen, Control>("_bgContainer");

	public static void TryTakeOver(NCharacterSelectScreen screen)
	{
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0174: Unknown result type (might be due to invalid IL or missing references)
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			Control val = _bgContainerRef.Invoke(screen);
			if (val == null)
			{
				return;
			}
			Node val2 = ((Node)val).FindChild("DanjinBgLetterbox", true, false);
			ColorRect letterbox = (ColorRect)(object)((val2 is ColorRect) ? val2 : null);
			if (letterbox == null)
			{
				return;
			}
			val2 = ((Node)letterbox).FindChild("DanjinBgVideo", true, false);
			VideoStreamPlayer video = (VideoStreamPlayer)(object)((val2 is VideoStreamPlayer) ? val2 : null);
			if (video == null)
			{
				return;
			}
			if (((GodotObject)letterbox).HasMeta(StringName.op_Implicit("_DanjinBgVideoTakenOver")))
			{
				ColorRect obj = letterbox;
				StringName obj2 = StringName.op_Implicit("_DanjinBgVideoTakenOver");
				Variant val3 = default(Variant);
				val3 = ((GodotObject)obj).GetMeta(obj2, val3);
				if (((Variant)(ref val3)).AsBool())
				{
					goto IL_013f;
				}
			}
			((CanvasItem)letterbox).TopLevel = false;
			((CanvasItem)video).TopLevel = false;
			video.Expand = true;
			Viewport viewport = ((Node)letterbox).GetViewport();
			Callable val4 = Callable.From((Action)delegate
			{
				if (GodotObject.IsInstanceValid((GodotObject)(object)letterbox) && GodotObject.IsInstanceValid((GodotObject)(object)video))
				{
					Fit(letterbox, video);
				}
			});
			if (!((GodotObject)viewport).IsConnected(SignalName.SizeChanged, val4))
			{
				((GodotObject)viewport).Connect(SignalName.SizeChanged, val4, 0u);
			}
			((GodotObject)letterbox).SetMeta(StringName.op_Implicit("_DanjinBgVideoTakenOver"), Variant.op_Implicit(true));
			DanjinLog.Verbose(">>>[DanjinMod] CharSelectBgVideo: 接管 (无 TopLevel, 反算父变换, 不遮 UI)");
			goto IL_013f;
			IL_013f:
			Fit(letterbox, video);
			ColorRect lb = letterbox;
			VideoStreamPlayer vd = video;
			Callable val5 = Callable.From((Action)delegate
			{
				if (GodotObject.IsInstanceValid((GodotObject)(object)lb) && GodotObject.IsInstanceValid((GodotObject)(object)vd))
				{
					Fit(lb, vd);
				}
			});
			((Callable)(ref val5)).CallDeferred(Array.Empty<Variant>());
			if (!video.IsPlaying())
			{
				video.Play();
			}
		}
		catch (Exception ex)
		{
			Log.Warn(">>>[DanjinMod] CharSelectBgVideo: TryTakeOver 异常 " + ex.GetType().Name + ": " + ex.Message, 1);
		}
	}

	private static void Fit(ColorRect letterbox, VideoStreamPlayer video)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		Node parent = ((Node)letterbox).GetParent();
		Control val = (Control)(object)((parent is Control) ? parent : null);
		if (val != null)
		{
			Rect2 visibleRect = ((Node)letterbox).GetViewport().GetVisibleRect();
			Vector2 position = ((Rect2)(ref visibleRect)).Position;
			Vector2 size = ((Rect2)(ref visibleRect)).Size;
			Transform2D globalTransformWithCanvas = ((CanvasItem)val).GetGlobalTransformWithCanvas();
			Vector2 position2 = ((Transform2D)(ref globalTransformWithCanvas)).AffineInverse() * position;
			Vector2 val2 = ((Transform2D)(ref globalTransformWithCanvas)).Scale;
			if (val2.X == 0f || val2.Y == 0f)
			{
				val2 = Vector2.One;
			}
			Vector2 val3 = size / val2;
			((Control)letterbox).Position = position2;
			((Control)letterbox).Size = val3;
			((Control)letterbox).Scale = Vector2.One;
			float num = 1.7777778f;
			float num2 = val3.X;
			float num3 = num2 / num;
			if (num3 < val3.Y)
			{
				num3 = val3.Y;
				num2 = num3 * num;
			}
			float num4 = (val3.X - num2) * 0.5f;
			float num5 = size.X / size.Y;
			float num6 = 0.5f;
			if (num5 > 1.78f)
			{
				float num7 = Mathf.Clamp((num5 - 1.78f) / 0.55999994f, 0f, 1f);
				num6 = Mathf.Lerp(0.5f, 0f, num7);
			}
			float num8 = (val3.Y - num3) * num6;
			((Control)video).Position = new Vector2(num4, num8);
			((Control)video).Size = new Vector2(num2, num3);
			((Control)video).Scale = Vector2.One;
		}
	}
}
