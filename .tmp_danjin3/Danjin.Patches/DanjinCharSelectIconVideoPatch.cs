using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Danjin.Character;
using Danjin.Extensions;
using Danjin.Utils;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect;

namespace Danjin.Patches;

[HarmonyPatch(typeof(NCharacterSelectScreen), "OnSubmenuOpened")]
public static class DanjinCharSelectIconVideoPatch
{
	private const string VideoFileName = "char_select_char_name.ogv";

	private const string VideoNodeName = "_DanjinVideoIcon";

	private const bool ShareHsvShader = true;

	private static readonly string[] FallbackPngs = new string[3] { "char_select_char_name_1.png", "char_select_char_name_2.png", "char_select_char_name_3.png" };

	private static readonly FieldRef<NCharacterSelectScreen, Control> _charButtonContainerRef = AccessTools.FieldRefAccess<NCharacterSelectScreen, Control>("_charButtonContainer");

	private static readonly FieldRef<NCharacterSelectButton, TextureRect> _iconRef = AccessTools.FieldRefAccess<NCharacterSelectButton, TextureRect>("_icon");

	private static readonly Random _rng = new Random();

	private static string? _lastPickedPath = null;

	private static VideoStream? _cachedStream;

	private static bool _videoLoadAttempted = false;

	private static bool _videoAvailable = false;

	[HarmonyPostfix]
	public static void Postfix(NCharacterSelectScreen __instance)
	{
		try
		{
			Control val = _charButtonContainerRef.Invoke(__instance);
			if (val == null)
			{
				Log.Warn(">>>[DanjinMod] CharSelectIconVideo: _charButtonContainer 反射失败", 1);
				return;
			}
			NCharacterSelectButton val2 = null;
			foreach (NCharacterSelectButton item in ((IEnumerable)((Node)val).GetChildren(false)).OfType<NCharacterSelectButton>())
			{
				if (item.Character is Danjin.Character.Danjin)
				{
					val2 = item;
					break;
				}
			}
			if (val2 == null)
			{
				return;
			}
			DanjinLog.Verbose($">>>[DanjinMod] CharSelectIconVideo: 命中 Danjin (locked={val2.IsLocked})");
			if (val2.IsLocked)
			{
				VideoStreamPlayer nodeOrNull = ((Node)val2).GetNodeOrNull<VideoStreamPlayer>(NodePath.op_Implicit("%Icon/_DanjinVideoIcon"));
				if (nodeOrNull != null)
				{
					((CanvasItem)nodeOrNull).Visible = false;
				}
				return;
			}
			TextureRect val3 = _iconRef.Invoke(val2);
			if (val3 == null)
			{
				Log.Warn(">>>[DanjinMod] CharSelectIconVideo: _icon 反射失败", 1);
			}
			else if (!TryEnsureVideoPlayer(val3))
			{
				FallbackPickRandomPng(val3);
			}
		}
		catch (Exception ex)
		{
			Log.Warn(">>>[DanjinMod] CharSelectIconVideo: OnSubmenuOpened 异常 " + ex.GetType().Name + ": " + ex.Message, 1);
		}
	}

	private static bool TryEnsureVideoPlayer(TextureRect icon)
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Expected O, but got Unknown
		if (!EnsureVideoStreamLoaded())
		{
			return false;
		}
		VideoStreamPlayer nodeOrNull = ((Node)icon).GetNodeOrNull<VideoStreamPlayer>(NodePath.op_Implicit("_DanjinVideoIcon"));
		if (nodeOrNull != null)
		{
			((CanvasItem)nodeOrNull).Visible = true;
			if (!nodeOrNull.IsPlaying())
			{
				nodeOrNull.Play();
			}
			return true;
		}
		VideoStreamPlayer val = new VideoStreamPlayer
		{
			Name = StringName.op_Implicit("_DanjinVideoIcon"),
			Stream = _cachedStream,
			Autoplay = true,
			Loop = true,
			Expand = true,
			VolumeDb = -80f,
			MouseFilter = (MouseFilterEnum)2
		};
		((Node)icon).AddChild((Node)(object)val, false, (InternalMode)0);
		((Control)val).SetAnchorsAndOffsetsPreset((LayoutPreset)15, (LayoutPresetMode)0, 0);
		Material material = ((CanvasItem)icon).Material;
		ShaderMaterial val2 = (ShaderMaterial)(object)((material is ShaderMaterial) ? material : null);
		if (val2 != null)
		{
			((CanvasItem)val).Material = (Material)(object)val2;
		}
		val.Play();
		DanjinLog.Verbose(">>>[DanjinMod] CharSelectIconVideo: 创建 VideoStreamPlayer (loop, autoplay)");
		return true;
	}

	private static bool EnsureVideoStreamLoaded()
	{
		if (_videoLoadAttempted)
		{
			return _videoAvailable;
		}
		_videoLoadAttempted = true;
		string text = "char_select_char_name.ogv".CharacterUiPath();
		if (!ResourceLoader.Exists(text, ""))
		{
			Log.Warn(">>>[DanjinMod] CharSelectIconVideo: 视频文件不存在 " + text + " —— 回退到 PNG 随机", 1);
			return false;
		}
		try
		{
			_cachedStream = ResourceLoader.Load<VideoStream>(text, (string)null, (CacheMode)1);
			_videoAvailable = _cachedStream != null;
			if (!_videoAvailable)
			{
				Log.Warn(">>>[DanjinMod] CharSelectIconVideo: 加载视频返回 null: " + text, 1);
			}
		}
		catch (Exception ex)
		{
			Log.Warn(">>>[DanjinMod] CharSelectIconVideo: 加载视频抛异常 " + ex.GetType().Name + ": " + ex.Message, 1);
			_videoAvailable = false;
		}
		return _videoAvailable;
	}

	private static void FallbackPickRandomPng(TextureRect icon)
	{
		List<string> list = (from name in FallbackPngs
			select name.CharacterUiPath() into p
			where ResourceLoader.Exists(p, "")
			select p).ToList();
		if (list.Count == 0)
		{
			return;
		}
		List<string> list2 = list;
		if (_lastPickedPath != null)
		{
			List<string> list3 = list.Where((string p) => p != _lastPickedPath).ToList();
			if (list3.Count > 0)
			{
				list2 = list3;
			}
		}
		string text = (_lastPickedPath = list2[_rng.Next(list2.Count)]);
		try
		{
			Texture2D val = ResourceLoader.Load<Texture2D>(text, (string)null, (CacheMode)1);
			if (val != null)
			{
				icon.Texture = val;
			}
		}
		catch (Exception ex)
		{
			Log.Warn(">>>[DanjinMod] CharSelectIconVideo: 加载兜底 PNG " + text + " 失败: " + ex.Message, 1);
		}
	}
}
