using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Danjin.Utils;
using Godot;
using Godot.Bridge;
using Godot.Collections;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace Danjin.Vfx;

[ScriptPath("res://Danjin/Vfx/NDimensionSlashVfx.cs")]
public class NDimensionSlashVfx : ColorRect
{
	public struct SlashOptions
	{
		public int Mode { get; set; }

		public float ExpandSlashDuration { get; set; }

		public float KeepSlashDuration { get; set; }

		public float ContractSlashDuration { get; set; }

		public float EachSlashShownInterval { get; set; }

		public float LineFadeInDuration { get; set; }

		public SlashOptions()
		{
			Mode = 0;
			ExpandSlashDuration = 0.5f;
			KeepSlashDuration = 0.5f;
			ContractSlashDuration = 0.5f;
			EachSlashShownInterval = 0.5f;
			LineFadeInDuration = 0.5f;
		}
	}

	public class MethodName : MethodName
	{
		public static readonly StringName Initialize = StringName.op_Implicit("Initialize");

		public static readonly StringName GenerateBlueNoiseAngles = StringName.op_Implicit("GenerateBlueNoiseAngles");

		public static readonly StringName _Ready = StringName.op_Implicit("_Ready");

		public static readonly StringName SetSlashColor = StringName.op_Implicit("SetSlashColor");

		public static readonly StringName SetSlashArea = StringName.op_Implicit("SetSlashArea");

		public static readonly StringName DoSlash = StringName.op_Implicit("DoSlash");

		public static readonly StringName ForceComplete = StringName.op_Implicit("ForceComplete");

		public static readonly StringName AnimateSingleLine = StringName.op_Implicit("AnimateSingleLine");

		public static readonly StringName AreaUVToScreenUV = StringName.op_Implicit("AreaUVToScreenUV");

		public static readonly StringName FlushLines = StringName.op_Implicit("FlushLines");
	}

	public class PropertyName : PropertyName
	{
		public static readonly StringName _mat = StringName.op_Implicit("_mat");

		public static readonly StringName _vpSize = StringName.op_Implicit("_vpSize");

		public static readonly StringName _areaPos = StringName.op_Implicit("_areaPos");

		public static readonly StringName _areaSize = StringName.op_Implicit("_areaSize");

		public static readonly StringName _backBufferCopySibling = StringName.op_Implicit("_backBufferCopySibling");

		public static readonly StringName _froms = StringName.op_Implicit("_froms");

		public static readonly StringName _tos = StringName.op_Implicit("_tos");

		public static readonly StringName _lineProgresses = StringName.op_Implicit("_lineProgresses");

		public static readonly StringName _lineAlphas = StringName.op_Implicit("_lineAlphas");

		public static readonly StringName _pendingTriggers = StringName.op_Implicit("_pendingTriggers");

		public static readonly StringName _nextTriggerSlot = StringName.op_Implicit("_nextTriggerSlot");

		public static readonly StringName _runningLineCount = StringName.op_Implicit("_runningLineCount");

		public static readonly StringName _completedLineCount = StringName.op_Implicit("_completedLineCount");

		public static readonly StringName _forceCompleteRequested = StringName.op_Implicit("_forceCompleteRequested");
	}

	public class SignalName : SignalName
	{
	}

	private static string? _slashScenePath;

	public const int MODE_ALL = 0;

	public const int MODE_SINGLE = 1;

	public const int MODE_TRIGGER = 2;

	public const int MODE_TRIGGER_SINGLE = 3;

	private ShaderMaterial? _mat;

	private Vector2 _vpSize;

	private Vector2 _areaPos;

	private Vector2 _areaSize;

	private static bool _matFailureReported;

	private BackBufferCopy? _backBufferCopySibling;

	private Vector2[]? _froms;

	private Vector2[]? _tos;

	private readonly float[] _lineProgresses = new float[64];

	private readonly float[] _lineAlphas = new float[64];

	private readonly bool[] _lineActive = new bool[64];

	private int _pendingTriggers;

	private TaskCompletionSource<bool>? _triggerTcs;

	private int _nextTriggerSlot;

	private int _runningLineCount;

	private int _completedLineCount;

	private TaskCompletionSource<bool>? _allDoneTcs;

	private bool _forceCompleteRequested;

	private SlashOptions Options { get; set; }

	public static void Initialize(string scenePath)
	{
		_slashScenePath = scenePath;
		if (!string.IsNullOrEmpty(scenePath))
		{
			try
			{
				ResourceLoader.Load<PackedScene>(scenePath, (string)null, (CacheMode)1);
			}
			catch (Exception ex)
			{
				Log.Warn("[Danjin] NDimensionSlashVfx 场景预热失败 (" + scenePath + "): " + ex.Message, 2);
			}
		}
	}

	public static NDimensionSlashVfx? Create(Vector2 areaGlobalPosition, Vector2 areaSize, SlashOptions options, Vector2[] froms, Vector2[] tos, Node? parentOverride = null)
	{
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Expected O, but got Unknown
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		if (string.IsNullOrEmpty(_slashScenePath))
		{
			Log.Warn("[Danjin] NDimensionSlashVfx 还没 Initialize, 跳过特效. 请在 mod 启动时调用 NDimensionSlashVfx.Initialize(scenePath).", 2);
			return null;
		}
		NDimensionSlashVfx nDimensionSlashVfx;
		try
		{
			nDimensionSlashVfx = PreloadManager.Cache.GetScene(_slashScenePath).Instantiate<NDimensionSlashVfx>((GenEditState)0);
		}
		catch (Exception ex)
		{
			Log.Error("[Danjin] NDimensionSlashVfx 场景实例化失败 (" + _slashScenePath + "): " + ex.Message, 2);
			return null;
		}
		Node val = (Node)(((object)parentOverride) ?? ((object)NCombatRoom.Instance));
		if (val != null)
		{
			BackBufferCopy val2 = new BackBufferCopy
			{
				CopyMode = (CopyModeEnum)2,
				Name = StringName.op_Implicit("DanjinSlashBackBufferCopy")
			};
			GodotTreeExtensions.AddChildSafely(val, (Node)(object)val2);
			GodotTreeExtensions.AddChildSafely(val, (Node)(object)nDimensionSlashVfx);
			nDimensionSlashVfx._backBufferCopySibling = val2;
			nDimensionSlashVfx.SetSlashArea(areaGlobalPosition, areaSize);
			nDimensionSlashVfx.SetSlashColor(Color.Color8(byte.MaxValue, (byte)81, (byte)49, byte.MaxValue));
		}
		nDimensionSlashVfx.Options = options;
		nDimensionSlashVfx._froms = froms;
		nDimensionSlashVfx._tos = tos;
		return nDimensionSlashVfx;
	}

	public static NDimensionSlashVfx? Create(NCreature nCreature, SlashOptions options, Vector2[] froms, Vector2[] tos)
	{
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Expected O, but got Unknown
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		if (string.IsNullOrEmpty(_slashScenePath))
		{
			Log.Warn("[Danjin] NDimensionSlashVfx 还没 Initialize, 跳过特效. 请在 mod 启动时调用 NDimensionSlashVfx.Initialize(scenePath).", 2);
			return null;
		}
		NDimensionSlashVfx nDimensionSlashVfx;
		try
		{
			nDimensionSlashVfx = PreloadManager.Cache.GetScene(_slashScenePath).Instantiate<NDimensionSlashVfx>((GenEditState)0);
		}
		catch (Exception ex)
		{
			Log.Error("[Danjin] NDimensionSlashVfx 场景实例化失败 (" + _slashScenePath + "): " + ex.Message, 2);
			return null;
		}
		NCombatRoom instance = NCombatRoom.Instance;
		if (instance != null)
		{
			BackBufferCopy val = new BackBufferCopy
			{
				CopyMode = (CopyModeEnum)2,
				Name = StringName.op_Implicit("DanjinSlashBackBufferCopy")
			};
			GodotTreeExtensions.AddChildSafely((Node)(object)instance, (Node)(object)val);
			GodotTreeExtensions.AddChildSafely((Node)(object)instance, (Node)(object)nDimensionSlashVfx);
			nDimensionSlashVfx._backBufferCopySibling = val;
			nDimensionSlashVfx.SetSlashArea(nCreature.Hitbox.GlobalPosition, nCreature.Hitbox.Size);
			nDimensionSlashVfx.SetSlashColor(Color.Color8(byte.MaxValue, (byte)81, (byte)49, byte.MaxValue));
		}
		nDimensionSlashVfx.Options = options;
		nDimensionSlashVfx._froms = froms;
		nDimensionSlashVfx._tos = tos;
		return nDimensionSlashVfx;
	}

	public static void GenerateRandomSlashLines(int count, float maxLength, float minLength, out Vector2[] froms, out Vector2[] tos, int seed = -1)
	{
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Expected O, but got Unknown
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		count = Mathf.Max(0, count);
		minLength = Mathf.Clamp(minLength, 0f, 1f);
		maxLength = Mathf.Clamp(maxLength, 0f, 1f);
		if (minLength > maxLength)
		{
			float num = maxLength;
			maxLength = minLength;
			minLength = num;
		}
		froms = (Vector2[])(object)new Vector2[count];
		tos = (Vector2[])(object)new Vector2[count];
		RandomNumberGenerator val = new RandomNumberGenerator();
		if (seed >= 0)
		{
			val.Seed = (ulong)seed;
		}
		else
		{
			val.Randomize();
		}
		Vector2 val2 = default(Vector2);
		for (int i = 0; i < count; i++)
		{
			float num2 = val.RandfRange(minLength, maxLength);
			float num3 = val.RandfRange(0f, (float)Math.PI * 2f);
			float num4 = num2 * Mathf.Cos(num3);
			float num5 = num2 * Mathf.Sin(num3);
			float num6 = Mathf.Max(0f, 0f - num4);
			float num7 = Mathf.Min(1f, 1f - num4);
			float num8 = Mathf.Max(0f, 0f - num5);
			float num9 = Mathf.Min(1f, 1f - num5);
			if (num6 > num7)
			{
				num6 = (num7 = Mathf.Clamp(0f - num4, 0f, 1f));
			}
			if (num8 > num9)
			{
				num8 = (num9 = Mathf.Clamp(0f - num5, 0f, 1f));
			}
			((Vector2)(ref val2))._002Ector(val.RandfRange(num6, num7), val.RandfRange(num8, num9));
			Vector2 val3 = val2 + new Vector2(num4, num5);
			froms[i] = val2;
			tos[i] = val3;
		}
	}

	public static void GenerateConvergingSlashLines(int count, float length, float pivotSpread, out Vector2[] froms, out Vector2[] tos, int seed = -1)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected O, but got Unknown
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		count = Mathf.Max(0, count);
		froms = (Vector2[])(object)new Vector2[count];
		tos = (Vector2[])(object)new Vector2[count];
		RandomNumberGenerator val = new RandomNumberGenerator();
		if (seed >= 0)
		{
			val.Seed = (ulong)seed;
		}
		else
		{
			val.Randomize();
		}
		Vector2 val2 = default(Vector2);
		((Vector2)(ref val2))._002Ector(0.5f, 0.5f);
		float[] array = null;
		if (count >= 2)
		{
			float minAngularSep = (float)Math.PI / (float)(count + 2);
			array = GenerateBlueNoiseAngles(1, count, minAngularSep, val);
		}
		Vector2 val4 = default(Vector2);
		for (int i = 0; i < count; i++)
		{
			Vector2 val3 = val2 + new Vector2(val.RandfRange(0f - pivotSpread, pivotSpread), val.RandfRange(0f - pivotSpread, pivotSpread));
			float num = ((array != null) ? array[i] : val.RandfRange(0f, (float)Math.PI));
			((Vector2)(ref val4))._002Ector(Mathf.Cos(num), Mathf.Sin(num));
			froms[i] = val3 - val4 * (length * 0.5f);
			tos[i] = val3 + val4 * (length * 0.5f);
		}
	}

	public static float[] GenerateBlueNoiseAngles(int n, int linesPerGroup, float minAngularSep, RandomNumberGenerator rng)
	{
		float[] array = new float[n * linesPerGroup];
		List<float>[] array2 = new List<float>[n];
		for (int i = 0; i < n; i++)
		{
			array2[i] = new List<float>(linesPerGroup);
		}
		for (int j = 0; j < linesPerGroup; j++)
		{
			for (int k = 0; k < n; k++)
			{
				List<float> list = array2[k];
				float num = 0f;
				for (int l = 0; l < 32; l++)
				{
					num = rng.RandfRange(0f, (float)Math.PI);
					bool flag = true;
					foreach (float item in list)
					{
						float num2 = Mathf.Abs(num - item);
						if (num2 > (float)Math.PI / 2f)
						{
							num2 = (float)Math.PI - num2;
						}
						if (num2 < minAngularSep)
						{
							flag = false;
							break;
						}
					}
					if (flag)
					{
						break;
					}
				}
				list.Add(num);
				array[j * n + k] = num;
			}
		}
		return array;
	}

	public override void _Ready()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected O, but got Unknown
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		Material material = ((CanvasItem)this).Material;
		ShaderMaterial val = (ShaderMaterial)(object)((material is ShaderMaterial) ? material : null);
		if (val != null)
		{
			_mat = (ShaderMaterial)((Resource)val).Duplicate(false);
			((CanvasItem)this).Material = (Material)(object)_mat;
		}
		else if (!_matFailureReported)
		{
			_matFailureReported = true;
			Log.Warn("[Danjin] NDimensionSlashVfx 场景缺少 ShaderMaterial, 特效不会显示. (后续相同错误将静默)", 2);
		}
		Rect2 visibleRect = ((Node)this).GetViewport().GetVisibleRect();
		_vpSize = ((Rect2)(ref visibleRect)).Size;
		_areaPos = Vector2.Zero;
		_areaSize = _vpSize;
		((Control)this).GlobalPosition = Vector2.Zero;
		((Control)this).Size = _vpSize;
		for (int i = 0; i < 64; i++)
		{
			_lineProgresses[i] = 0.5f;
			_lineAlphas[i] = 0f;
		}
		((Node)this).GetViewport().SizeChanged += delegate
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			Rect2 visibleRect2 = ((Node)this).GetViewport().GetVisibleRect();
			_vpSize = ((Rect2)(ref visibleRect2)).Size;
			((Control)this).GlobalPosition = Vector2.Zero;
			((Control)this).Size = _vpSize;
		};
		((Node)this).TreeExited += delegate
		{
			if (_backBufferCopySibling != null && GodotObject.IsInstanceValid((GodotObject)(object)_backBufferCopySibling))
			{
				((Node)_backBufferCopySibling).QueueFree();
			}
		};
	}

	public void SetSlashColor(Color color)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		ShaderMaterial? mat = _mat;
		if (mat != null)
		{
			mat.SetShaderParameter(StringName.op_Implicit("glow_color"), Variant.op_Implicit(color));
		}
	}

	public void SetSlashArea(Vector2 globalPosition, Vector2 size)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		_areaPos = globalPosition;
		_areaSize = size;
	}

	public async Task TriggerSlash()
	{
		if (Options.Mode == 0)
		{
			await RunModeAll();
		}
		else if (Options.Mode == 1)
		{
			await RunModeSingle();
		}
		else if (Options.Mode == 2)
		{
			await RunModeTrigger();
		}
		else if (Options.Mode == 3)
		{
			await RunModeTriggerSingle();
		}
		if (_backBufferCopySibling != null && GodotObject.IsInstanceValid((GodotObject)(object)_backBufferCopySibling))
		{
			((Node)_backBufferCopySibling).QueueFree();
		}
		((Node)this).QueueFree();
	}

	public void DoSlash(int count = 1)
	{
		if (Options.Mode == 2)
		{
			int num = ((_froms != null) ? Mathf.Min(_froms.Length, 64) : 0) - (_nextTriggerSlot + _pendingTriggers);
			if (num > 0)
			{
				_pendingTriggers += Mathf.Min(count, num);
				_triggerTcs?.TrySetResult(result: true);
			}
		}
		else
		{
			if (Options.Mode != 3)
			{
				return;
			}
			int num2 = ((_froms != null) ? Mathf.Min(_froms.Length, 64) : 0);
			for (int i = 0; i < count; i++)
			{
				if (_nextTriggerSlot >= num2)
				{
					break;
				}
				AnimateSingleLine(_nextTriggerSlot++);
			}
		}
	}

	public void ForceComplete()
	{
		if (_froms == null)
		{
			return;
		}
		if (Options.Mode == 3)
		{
			_forceCompleteRequested = true;
		}
		if (_allDoneTcs == null || _allDoneTcs.Task.IsCompleted)
		{
			return;
		}
		int num = Mathf.Min(_froms.Length, 64);
		if (Options.Mode == 3)
		{
			while (_nextTriggerSlot < num)
			{
				_nextTriggerSlot++;
				_completedLineCount++;
			}
			if (_runningLineCount == 0)
			{
				_allDoneTcs.TrySetResult(result: true);
			}
		}
		else if (Options.Mode == 2)
		{
			_pendingTriggers = 0;
			_allDoneTcs.TrySetResult(result: true);
		}
	}

	private async Task RunModeAll()
	{
		if (_froms == null || _mat == null)
		{
			return;
		}
		int count = Mathf.Min(_froms.Length, 64);
		for (int i = 0; i < count; i++)
		{
			_lineActive[i] = true;
			_lineProgresses[i] = 0f;
			_lineAlphas[i] = 0f;
		}
		_mat.SetShaderParameter(StringName.op_Implicit("slash_progress"), Variant.op_Implicit(0f));
		FlushLines();
		Tween val = ((Node)this).CreateTween().SetParallel(true);
		val.TweenMethod(Callable.From<float>((Action<float>)delegate(float v)
		{
			for (int j = 0; j < count; j++)
			{
				_lineProgresses[j] = v;
			}
			FlushLines();
		}), Variant.op_Implicit(0f), Variant.op_Implicit(0.5f), (double)Options.ExpandSlashDuration);
		val.TweenMethod(Callable.From<float>((Action<float>)delegate(float v)
		{
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			_mat.SetShaderParameter(StringName.op_Implicit("slash_progress"), Variant.op_Implicit(v));
		}), Variant.op_Implicit(0f), Variant.op_Implicit(1f), (double)Options.ExpandSlashDuration);
		float num = Mathf.Max(0.0001f, Options.LineFadeInDuration);
		val.TweenMethod(Callable.From<float>((Action<float>)delegate(float v)
		{
			for (int j = 0; j < count; j++)
			{
				_lineAlphas[j] = v;
			}
			FlushLines();
		}), Variant.op_Implicit(0f), Variant.op_Implicit(1f), (double)num);
		await ((GodotObject)this).ToSignal((GodotObject)(object)val, SignalName.Finished);
		await ((GodotObject)this).ToSignal((GodotObject)(object)((Node)this).GetTree().CreateTimer((double)Options.KeepSlashDuration, true, false, true), SignalName.Timeout);
		Tween val2 = ((Node)this).CreateTween();
		val2.TweenMethod(Callable.From<float>((Action<float>)delegate(float v)
		{
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			_mat.SetShaderParameter(StringName.op_Implicit("slash_progress"), Variant.op_Implicit(v));
		}), Variant.op_Implicit(1f), Variant.op_Implicit(0f), (double)Options.ContractSlashDuration);
		await ((GodotObject)this).ToSignal((GodotObject)(object)val2, SignalName.Finished);
		for (int num2 = 0; num2 < count; num2++)
		{
			_lineActive[num2] = false;
			_lineAlphas[num2] = 0f;
			_lineProgresses[num2] = 0.5f;
		}
		FlushLines();
	}

	private async Task RunModeSingle()
	{
		if (_froms == null || _mat == null)
		{
			return;
		}
		int total = Mathf.Min(_froms.Length, 64);
		_mat.SetShaderParameter(StringName.op_Implicit("slash_progress"), Variant.op_Implicit(0f));
		for (int i = 0; i < total; i++)
		{
			_lineActive[i] = true;
			_lineProgresses[i] = 0f;
			_lineAlphas[i] = 0f;
			int captured = i;
			Tween val = ((Node)this).CreateTween().SetParallel(true);
			val.TweenMethod(Callable.From<float>((Action<float>)delegate(float v)
			{
				_lineProgresses[captured] = v;
				FlushLines();
			}), Variant.op_Implicit(0f), Variant.op_Implicit(0.5f), (double)Options.ExpandSlashDuration);
			val.TweenMethod(Callable.From<float>((Action<float>)delegate(float v)
			{
				_lineAlphas[captured] = v;
				FlushLines();
			}), Variant.op_Implicit(0f), Variant.op_Implicit(1f), (double)Mathf.Max(0.0001f, Options.LineFadeInDuration));
			if (i == 0)
			{
				val.TweenMethod(Callable.From<float>((Action<float>)delegate(float v)
				{
					//IL_0011: Unknown result type (might be due to invalid IL or missing references)
					_mat.SetShaderParameter(StringName.op_Implicit("slash_progress"), Variant.op_Implicit(v));
				}), Variant.op_Implicit(0f), Variant.op_Implicit(1f), (double)Options.ExpandSlashDuration);
			}
			await ((GodotObject)this).ToSignal((GodotObject)(object)val, SignalName.Finished);
			if (i < total - 1)
			{
				await ((GodotObject)this).ToSignal((GodotObject)(object)((Node)this).GetTree().CreateTimer((double)Options.EachSlashShownInterval, true, false, true), SignalName.Timeout);
			}
		}
		await ((GodotObject)this).ToSignal((GodotObject)(object)((Node)this).GetTree().CreateTimer((double)Options.KeepSlashDuration, true, false, true), SignalName.Timeout);
		Tween val2 = ((Node)this).CreateTween();
		val2.TweenMethod(Callable.From<float>((Action<float>)delegate(float v)
		{
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			_mat.SetShaderParameter(StringName.op_Implicit("slash_progress"), Variant.op_Implicit(v));
		}), Variant.op_Implicit(1f), Variant.op_Implicit(0f), (double)Options.ContractSlashDuration);
		await ((GodotObject)this).ToSignal((GodotObject)(object)val2, SignalName.Finished);
		for (int num = 0; num < total; num++)
		{
			_lineActive[num] = false;
			_lineAlphas[num] = 0f;
			_lineProgresses[num] = 0.5f;
		}
		FlushLines();
	}

	private async Task RunModeTrigger()
	{
		if (_froms == null || _mat == null)
		{
			return;
		}
		int total = Mathf.Min(_froms.Length, 64);
		_mat.SetShaderParameter(StringName.op_Implicit("slash_progress"), Variant.op_Implicit(0f));
		for (int i = 0; i < total; i++)
		{
			await ConsumeOneTrigger();
			_nextTriggerSlot = i + 1;
			_lineActive[i] = true;
			_lineProgresses[i] = 0f;
			_lineAlphas[i] = 0f;
			int captured = i;
			Tween val = ((Node)this).CreateTween().SetParallel(true);
			val.TweenMethod(Callable.From<float>((Action<float>)delegate(float v)
			{
				_lineProgresses[captured] = v;
				FlushLines();
			}), Variant.op_Implicit(0f), Variant.op_Implicit(0.5f), (double)Options.ExpandSlashDuration);
			val.TweenMethod(Callable.From<float>((Action<float>)delegate(float v)
			{
				_lineAlphas[captured] = v;
				FlushLines();
			}), Variant.op_Implicit(0f), Variant.op_Implicit(1f), (double)Mathf.Max(0.0001f, Options.LineFadeInDuration));
			if (i == 0)
			{
				val.TweenMethod(Callable.From<float>((Action<float>)delegate(float v)
				{
					//IL_0011: Unknown result type (might be due to invalid IL or missing references)
					_mat.SetShaderParameter(StringName.op_Implicit("slash_progress"), Variant.op_Implicit(v));
				}), Variant.op_Implicit(0f), Variant.op_Implicit(1f), (double)Options.ExpandSlashDuration);
			}
			await ((GodotObject)this).ToSignal((GodotObject)(object)val, SignalName.Finished);
		}
		await ((GodotObject)this).ToSignal((GodotObject)(object)((Node)this).GetTree().CreateTimer((double)Options.KeepSlashDuration, true, false, true), SignalName.Timeout);
		Tween val2 = ((Node)this).CreateTween();
		val2.TweenMethod(Callable.From<float>((Action<float>)delegate(float v)
		{
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			_mat.SetShaderParameter(StringName.op_Implicit("slash_progress"), Variant.op_Implicit(v));
		}), Variant.op_Implicit(1f), Variant.op_Implicit(0f), (double)Options.ContractSlashDuration);
		await ((GodotObject)this).ToSignal((GodotObject)(object)val2, SignalName.Finished);
		for (int num = 0; num < total; num++)
		{
			_lineActive[num] = false;
			_lineAlphas[num] = 0f;
			_lineProgresses[num] = 0.5f;
		}
		FlushLines();
	}

	private async Task RunModeTriggerSingle()
	{
		if (_froms != null && _mat != null)
		{
			_allDoneTcs = new TaskCompletionSource<bool>();
			_mat.SetShaderParameter(StringName.op_Implicit("slash_progress"), Variant.op_Implicit(0f));
			await _allDoneTcs.Task;
			Tween val = ((Node)this).CreateTween();
			val.TweenMethod(Callable.From<float>((Action<float>)delegate(float v)
			{
				//IL_0011: Unknown result type (might be due to invalid IL or missing references)
				_mat.SetShaderParameter(StringName.op_Implicit("slash_progress"), Variant.op_Implicit(v));
			}), Variant.op_Implicit(1f), Variant.op_Implicit(0f), (double)Options.ContractSlashDuration);
			await ((GodotObject)this).ToSignal((GodotObject)(object)val, SignalName.Finished);
		}
	}

	private async void AnimateSingleLine(int slot)
	{
		if (_froms == null || _mat == null)
		{
			return;
		}
		Mathf.Min(_froms.Length, 64);
		_lineActive[slot] = true;
		_lineProgresses[slot] = 0f;
		_lineAlphas[slot] = 0f;
		_runningLineCount++;
		bool num = _runningLineCount == 1;
		Tween val = ((Node)this).CreateTween().SetParallel(true);
		val.TweenMethod(Callable.From<float>((Action<float>)delegate(float v)
		{
			_lineProgresses[slot] = v;
			FlushLines();
		}), Variant.op_Implicit(0f), Variant.op_Implicit(0.5f), (double)Options.ExpandSlashDuration);
		val.TweenMethod(Callable.From<float>((Action<float>)delegate(float v)
		{
			_lineAlphas[slot] = v;
			FlushLines();
		}), Variant.op_Implicit(0f), Variant.op_Implicit(1f), (double)Mathf.Max(0.0001f, Options.LineFadeInDuration));
		if (num)
		{
			val.TweenMethod(Callable.From<float>((Action<float>)delegate(float v)
			{
				//IL_0016: Unknown result type (might be due to invalid IL or missing references)
				_mat.SetShaderParameter(StringName.op_Implicit("slash_progress"), Variant.op_Implicit(v));
			}), Variant.op_Implicit(0f), Variant.op_Implicit(1f), (double)Options.ExpandSlashDuration);
		}
		await ((GodotObject)this).ToSignal((GodotObject)(object)val, SignalName.Finished);
		await ((GodotObject)this).ToSignal((GodotObject)(object)((Node)this).GetTree().CreateTimer((double)Options.KeepSlashDuration, true, false, true), SignalName.Timeout);
		Tween val2 = ((Node)this).CreateTween();
		val2.TweenMethod(Callable.From<float>((Action<float>)delegate(float v)
		{
			_lineAlphas[slot] = v;
			FlushLines();
		}), Variant.op_Implicit(1f), Variant.op_Implicit(0f), (double)Options.ContractSlashDuration);
		await ((GodotObject)this).ToSignal((GodotObject)(object)val2, SignalName.Finished);
		_lineActive[slot] = false;
		_lineAlphas[slot] = 0f;
		_lineProgresses[slot] = 0.5f;
		FlushLines();
		_runningLineCount--;
		_completedLineCount++;
		if (_forceCompleteRequested && _runningLineCount == 0)
		{
			_allDoneTcs?.TrySetResult(result: true);
		}
	}

	private async Task ConsumeOneTrigger()
	{
		while (_pendingTriggers <= 0)
		{
			_triggerTcs = new TaskCompletionSource<bool>();
			await _triggerTcs.Task;
			_triggerTcs = null;
		}
		_pendingTriggers--;
	}

	private Vector2 AreaUVToScreenUV(Vector2 areaUV)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		return (_areaPos + areaUV * _areaSize) / _vpSize;
	}

	private void FlushLines()
	{
		//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01be: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_022c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		if (_mat == null)
		{
			return;
		}
		Array<Vector2> val = new Array<Vector2>();
		Array<Vector2> val2 = new Array<Vector2>();
		Array<float> val3 = new Array<float>();
		Array<float> val4 = new Array<float>();
		for (int i = 0; i < 64; i++)
		{
			if (_lineActive[i] && _froms != null && i < _froms.Length && _tos != null)
			{
				Vector2 val5 = AreaUVToScreenUV(_froms[i]);
				Vector2 val6 = AreaUVToScreenUV(_tos[i]);
				DanjinLog.Verbose($"[SlashVfx] Line{i} From={val5} To={val6} lp={_lineProgresses[i]} a={_lineAlphas[i]}");
				val.Add(val5);
				val2.Add(val6);
				val3.Add(_lineAlphas[i]);
				val4.Add(_lineProgresses[i]);
			}
			else
			{
				val.Add(new Vector2(-5f, -5f));
				val2.Add(new Vector2(-5f, -5f));
				val3.Add(0f);
				val4.Add(0.5f);
			}
		}
		_mat.SetShaderParameter(StringName.op_Implicit("slash_points_a"), Array<Vector2>.op_Implicit(val));
		_mat.SetShaderParameter(StringName.op_Implicit("slash_points_b"), Array<Vector2>.op_Implicit(val2));
		_mat.SetShaderParameter(StringName.op_Implicit("slash_weights"), Array<float>.op_Implicit(val3));
		_mat.SetShaderParameter(StringName.op_Implicit("slash_line_progress"), Array<float>.op_Implicit(val4));
		int num = ((_froms != null) ? Mathf.Min(_froms.Length, 64) : 0);
		_mat.SetShaderParameter(StringName.op_Implicit("slash_count"), Variant.op_Implicit(num));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Expected O, but got Unknown
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		//IL_018f: Unknown result type (might be due to invalid IL or missing references)
		//IL_019a: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0204: Unknown result type (might be due to invalid IL or missing references)
		//IL_020f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0235: Unknown result type (might be due to invalid IL or missing references)
		//IL_0258: Unknown result type (might be due to invalid IL or missing references)
		//IL_0263: Unknown result type (might be due to invalid IL or missing references)
		//IL_0289: Unknown result type (might be due to invalid IL or missing references)
		//IL_0292: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02db: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_030c: Unknown result type (might be due to invalid IL or missing references)
		//IL_032f: Unknown result type (might be due to invalid IL or missing references)
		//IL_033a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0360: Unknown result type (might be due to invalid IL or missing references)
		//IL_0369: Unknown result type (might be due to invalid IL or missing references)
		return new List<MethodInfo>(10)
		{
			new MethodInfo(MethodName.Initialize, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)33, new List<PropertyInfo>
			{
				new PropertyInfo((Type)4, StringName.op_Implicit("scenePath"), (PropertyHint)0, "", (PropertyUsageFlags)6, false)
			}, (List<Variant>)null),
			new MethodInfo(MethodName.GenerateBlueNoiseAngles, new PropertyInfo((Type)32, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)33, new List<PropertyInfo>
			{
				new PropertyInfo((Type)2, StringName.op_Implicit("n"), (PropertyHint)0, "", (PropertyUsageFlags)6, false),
				new PropertyInfo((Type)2, StringName.op_Implicit("linesPerGroup"), (PropertyHint)0, "", (PropertyUsageFlags)6, false),
				new PropertyInfo((Type)3, StringName.op_Implicit("minAngularSep"), (PropertyHint)0, "", (PropertyUsageFlags)6, false),
				new PropertyInfo((Type)24, StringName.op_Implicit("rng"), (PropertyHint)0, "", (PropertyUsageFlags)6, new StringName("RandomNumberGenerator"), false)
			}, (List<Variant>)null),
			new MethodInfo(MethodName._Ready, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, (List<PropertyInfo>)null, (List<Variant>)null),
			new MethodInfo(MethodName.SetSlashColor, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, new List<PropertyInfo>
			{
				new PropertyInfo((Type)20, StringName.op_Implicit("color"), (PropertyHint)0, "", (PropertyUsageFlags)6, false)
			}, (List<Variant>)null),
			new MethodInfo(MethodName.SetSlashArea, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, new List<PropertyInfo>
			{
				new PropertyInfo((Type)5, StringName.op_Implicit("globalPosition"), (PropertyHint)0, "", (PropertyUsageFlags)6, false),
				new PropertyInfo((Type)5, StringName.op_Implicit("size"), (PropertyHint)0, "", (PropertyUsageFlags)6, false)
			}, (List<Variant>)null),
			new MethodInfo(MethodName.DoSlash, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, new List<PropertyInfo>
			{
				new PropertyInfo((Type)2, StringName.op_Implicit("count"), (PropertyHint)0, "", (PropertyUsageFlags)6, false)
			}, (List<Variant>)null),
			new MethodInfo(MethodName.ForceComplete, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, (List<PropertyInfo>)null, (List<Variant>)null),
			new MethodInfo(MethodName.AnimateSingleLine, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, new List<PropertyInfo>
			{
				new PropertyInfo((Type)2, StringName.op_Implicit("slot"), (PropertyHint)0, "", (PropertyUsageFlags)6, false)
			}, (List<Variant>)null),
			new MethodInfo(MethodName.AreaUVToScreenUV, new PropertyInfo((Type)5, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, new List<PropertyInfo>
			{
				new PropertyInfo((Type)5, StringName.op_Implicit("areaUV"), (PropertyHint)0, "", (PropertyUsageFlags)6, false)
			}, (List<Variant>)null),
			new MethodInfo(MethodName.FlushLines, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, (List<PropertyInfo>)null, (List<Variant>)null)
		};
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_0216: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01da: Unknown result type (might be due to invalid IL or missing references)
		//IL_01df: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_020c: Unknown result type (might be due to invalid IL or missing references)
		if ((ref method) == MethodName.Initialize && ((NativeVariantPtrArgs)(ref args)).Count == 1)
		{
			Initialize(VariantUtils.ConvertTo<string>(ref ((NativeVariantPtrArgs)(ref args))[0]));
			ret = default(godot_variant);
			return true;
		}
		if ((ref method) == MethodName.GenerateBlueNoiseAngles && ((NativeVariantPtrArgs)(ref args)).Count == 4)
		{
			float[] array = GenerateBlueNoiseAngles(VariantUtils.ConvertTo<int>(ref ((NativeVariantPtrArgs)(ref args))[0]), VariantUtils.ConvertTo<int>(ref ((NativeVariantPtrArgs)(ref args))[1]), VariantUtils.ConvertTo<float>(ref ((NativeVariantPtrArgs)(ref args))[2]), VariantUtils.ConvertTo<RandomNumberGenerator>(ref ((NativeVariantPtrArgs)(ref args))[3]));
			ret = VariantUtils.CreateFrom<float[]>(ref array);
			return true;
		}
		if ((ref method) == MethodName._Ready && ((NativeVariantPtrArgs)(ref args)).Count == 0)
		{
			((Node)this)._Ready();
			ret = default(godot_variant);
			return true;
		}
		if ((ref method) == MethodName.SetSlashColor && ((NativeVariantPtrArgs)(ref args)).Count == 1)
		{
			SetSlashColor(VariantUtils.ConvertTo<Color>(ref ((NativeVariantPtrArgs)(ref args))[0]));
			ret = default(godot_variant);
			return true;
		}
		if ((ref method) == MethodName.SetSlashArea && ((NativeVariantPtrArgs)(ref args)).Count == 2)
		{
			SetSlashArea(VariantUtils.ConvertTo<Vector2>(ref ((NativeVariantPtrArgs)(ref args))[0]), VariantUtils.ConvertTo<Vector2>(ref ((NativeVariantPtrArgs)(ref args))[1]));
			ret = default(godot_variant);
			return true;
		}
		if ((ref method) == MethodName.DoSlash && ((NativeVariantPtrArgs)(ref args)).Count == 1)
		{
			DoSlash(VariantUtils.ConvertTo<int>(ref ((NativeVariantPtrArgs)(ref args))[0]));
			ret = default(godot_variant);
			return true;
		}
		if ((ref method) == MethodName.ForceComplete && ((NativeVariantPtrArgs)(ref args)).Count == 0)
		{
			ForceComplete();
			ret = default(godot_variant);
			return true;
		}
		if ((ref method) == MethodName.AnimateSingleLine && ((NativeVariantPtrArgs)(ref args)).Count == 1)
		{
			AnimateSingleLine(VariantUtils.ConvertTo<int>(ref ((NativeVariantPtrArgs)(ref args))[0]));
			ret = default(godot_variant);
			return true;
		}
		if ((ref method) == MethodName.AreaUVToScreenUV && ((NativeVariantPtrArgs)(ref args)).Count == 1)
		{
			Vector2 val = AreaUVToScreenUV(VariantUtils.ConvertTo<Vector2>(ref ((NativeVariantPtrArgs)(ref args))[0]));
			ret = VariantUtils.CreateFrom<Vector2>(ref val);
			return true;
		}
		if ((ref method) == MethodName.FlushLines && ((NativeVariantPtrArgs)(ref args)).Count == 0)
		{
			FlushLines();
			ret = default(godot_variant);
			return true;
		}
		return ((ColorRect)this).InvokeGodotClassMethod(ref method, args, ref ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static bool InvokeGodotClassStaticMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		if ((ref method) == MethodName.Initialize && ((NativeVariantPtrArgs)(ref args)).Count == 1)
		{
			Initialize(VariantUtils.ConvertTo<string>(ref ((NativeVariantPtrArgs)(ref args))[0]));
			ret = default(godot_variant);
			return true;
		}
		if ((ref method) == MethodName.GenerateBlueNoiseAngles && ((NativeVariantPtrArgs)(ref args)).Count == 4)
		{
			float[] array = GenerateBlueNoiseAngles(VariantUtils.ConvertTo<int>(ref ((NativeVariantPtrArgs)(ref args))[0]), VariantUtils.ConvertTo<int>(ref ((NativeVariantPtrArgs)(ref args))[1]), VariantUtils.ConvertTo<float>(ref ((NativeVariantPtrArgs)(ref args))[2]), VariantUtils.ConvertTo<RandomNumberGenerator>(ref ((NativeVariantPtrArgs)(ref args))[3]));
			ret = VariantUtils.CreateFrom<float[]>(ref array);
			return true;
		}
		ret = default(godot_variant);
		return false;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if ((ref method) == MethodName.Initialize)
		{
			return true;
		}
		if ((ref method) == MethodName.GenerateBlueNoiseAngles)
		{
			return true;
		}
		if ((ref method) == MethodName._Ready)
		{
			return true;
		}
		if ((ref method) == MethodName.SetSlashColor)
		{
			return true;
		}
		if ((ref method) == MethodName.SetSlashArea)
		{
			return true;
		}
		if ((ref method) == MethodName.DoSlash)
		{
			return true;
		}
		if ((ref method) == MethodName.ForceComplete)
		{
			return true;
		}
		if ((ref method) == MethodName.AnimateSingleLine)
		{
			return true;
		}
		if ((ref method) == MethodName.AreaUVToScreenUV)
		{
			return true;
		}
		if ((ref method) == MethodName.FlushLines)
		{
			return true;
		}
		return ((ColorRect)this).HasGodotClassMethod(ref method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		if ((ref name) == PropertyName._mat)
		{
			_mat = VariantUtils.ConvertTo<ShaderMaterial>(ref value);
			return true;
		}
		if ((ref name) == PropertyName._vpSize)
		{
			_vpSize = VariantUtils.ConvertTo<Vector2>(ref value);
			return true;
		}
		if ((ref name) == PropertyName._areaPos)
		{
			_areaPos = VariantUtils.ConvertTo<Vector2>(ref value);
			return true;
		}
		if ((ref name) == PropertyName._areaSize)
		{
			_areaSize = VariantUtils.ConvertTo<Vector2>(ref value);
			return true;
		}
		if ((ref name) == PropertyName._backBufferCopySibling)
		{
			_backBufferCopySibling = VariantUtils.ConvertTo<BackBufferCopy>(ref value);
			return true;
		}
		if ((ref name) == PropertyName._froms)
		{
			_froms = VariantUtils.ConvertTo<Vector2[]>(ref value);
			return true;
		}
		if ((ref name) == PropertyName._tos)
		{
			_tos = VariantUtils.ConvertTo<Vector2[]>(ref value);
			return true;
		}
		if ((ref name) == PropertyName._pendingTriggers)
		{
			_pendingTriggers = VariantUtils.ConvertTo<int>(ref value);
			return true;
		}
		if ((ref name) == PropertyName._nextTriggerSlot)
		{
			_nextTriggerSlot = VariantUtils.ConvertTo<int>(ref value);
			return true;
		}
		if ((ref name) == PropertyName._runningLineCount)
		{
			_runningLineCount = VariantUtils.ConvertTo<int>(ref value);
			return true;
		}
		if ((ref name) == PropertyName._completedLineCount)
		{
			_completedLineCount = VariantUtils.ConvertTo<int>(ref value);
			return true;
		}
		if ((ref name) == PropertyName._forceCompleteRequested)
		{
			_forceCompleteRequested = VariantUtils.ConvertTo<bool>(ref value);
			return true;
		}
		return ((GodotObject)this).SetGodotClassPropertyValue(ref name, ref value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_0174: Unknown result type (might be due to invalid IL or missing references)
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		//IL_0194: Unknown result type (might be due to invalid IL or missing references)
		//IL_0199: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
		if ((ref name) == PropertyName._mat)
		{
			value = VariantUtils.CreateFrom<ShaderMaterial>(ref _mat);
			return true;
		}
		if ((ref name) == PropertyName._vpSize)
		{
			value = VariantUtils.CreateFrom<Vector2>(ref _vpSize);
			return true;
		}
		if ((ref name) == PropertyName._areaPos)
		{
			value = VariantUtils.CreateFrom<Vector2>(ref _areaPos);
			return true;
		}
		if ((ref name) == PropertyName._areaSize)
		{
			value = VariantUtils.CreateFrom<Vector2>(ref _areaSize);
			return true;
		}
		if ((ref name) == PropertyName._backBufferCopySibling)
		{
			value = VariantUtils.CreateFrom<BackBufferCopy>(ref _backBufferCopySibling);
			return true;
		}
		if ((ref name) == PropertyName._froms)
		{
			value = VariantUtils.CreateFrom<Vector2[]>(ref _froms);
			return true;
		}
		if ((ref name) == PropertyName._tos)
		{
			value = VariantUtils.CreateFrom<Vector2[]>(ref _tos);
			return true;
		}
		if ((ref name) == PropertyName._lineProgresses)
		{
			value = VariantUtils.CreateFrom<float[]>(ref _lineProgresses);
			return true;
		}
		if ((ref name) == PropertyName._lineAlphas)
		{
			value = VariantUtils.CreateFrom<float[]>(ref _lineAlphas);
			return true;
		}
		if ((ref name) == PropertyName._pendingTriggers)
		{
			value = VariantUtils.CreateFrom<int>(ref _pendingTriggers);
			return true;
		}
		if ((ref name) == PropertyName._nextTriggerSlot)
		{
			value = VariantUtils.CreateFrom<int>(ref _nextTriggerSlot);
			return true;
		}
		if ((ref name) == PropertyName._runningLineCount)
		{
			value = VariantUtils.CreateFrom<int>(ref _runningLineCount);
			return true;
		}
		if ((ref name) == PropertyName._completedLineCount)
		{
			value = VariantUtils.CreateFrom<int>(ref _completedLineCount);
			return true;
		}
		if ((ref name) == PropertyName._forceCompleteRequested)
		{
			value = VariantUtils.CreateFrom<bool>(ref _forceCompleteRequested);
			return true;
		}
		return ((GodotObject)this).GetGodotClassPropertyValue(ref name, ref value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c1: Unknown result type (might be due to invalid IL or missing references)
		return new List<PropertyInfo>
		{
			new PropertyInfo((Type)24, PropertyName._mat, (PropertyHint)0, "", (PropertyUsageFlags)4096, false),
			new PropertyInfo((Type)5, PropertyName._vpSize, (PropertyHint)0, "", (PropertyUsageFlags)4096, false),
			new PropertyInfo((Type)5, PropertyName._areaPos, (PropertyHint)0, "", (PropertyUsageFlags)4096, false),
			new PropertyInfo((Type)5, PropertyName._areaSize, (PropertyHint)0, "", (PropertyUsageFlags)4096, false),
			new PropertyInfo((Type)24, PropertyName._backBufferCopySibling, (PropertyHint)0, "", (PropertyUsageFlags)4096, false),
			new PropertyInfo((Type)35, PropertyName._froms, (PropertyHint)0, "", (PropertyUsageFlags)4096, false),
			new PropertyInfo((Type)35, PropertyName._tos, (PropertyHint)0, "", (PropertyUsageFlags)4096, false),
			new PropertyInfo((Type)32, PropertyName._lineProgresses, (PropertyHint)0, "", (PropertyUsageFlags)4096, false),
			new PropertyInfo((Type)32, PropertyName._lineAlphas, (PropertyHint)0, "", (PropertyUsageFlags)4096, false),
			new PropertyInfo((Type)2, PropertyName._pendingTriggers, (PropertyHint)0, "", (PropertyUsageFlags)4096, false),
			new PropertyInfo((Type)2, PropertyName._nextTriggerSlot, (PropertyHint)0, "", (PropertyUsageFlags)4096, false),
			new PropertyInfo((Type)2, PropertyName._runningLineCount, (PropertyHint)0, "", (PropertyUsageFlags)4096, false),
			new PropertyInfo((Type)2, PropertyName._completedLineCount, (PropertyHint)0, "", (PropertyUsageFlags)4096, false),
			new PropertyInfo((Type)1, PropertyName._forceCompleteRequested, (PropertyHint)0, "", (PropertyUsageFlags)4096, false)
		};
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		((GodotObject)this).SaveGodotObjectData(info);
		info.AddProperty(PropertyName._mat, Variant.From<ShaderMaterial>(ref _mat));
		info.AddProperty(PropertyName._vpSize, Variant.From<Vector2>(ref _vpSize));
		info.AddProperty(PropertyName._areaPos, Variant.From<Vector2>(ref _areaPos));
		info.AddProperty(PropertyName._areaSize, Variant.From<Vector2>(ref _areaSize));
		info.AddProperty(PropertyName._backBufferCopySibling, Variant.From<BackBufferCopy>(ref _backBufferCopySibling));
		info.AddProperty(PropertyName._froms, Variant.From<Vector2[]>(ref _froms));
		info.AddProperty(PropertyName._tos, Variant.From<Vector2[]>(ref _tos));
		info.AddProperty(PropertyName._pendingTriggers, Variant.From<int>(ref _pendingTriggers));
		info.AddProperty(PropertyName._nextTriggerSlot, Variant.From<int>(ref _nextTriggerSlot));
		info.AddProperty(PropertyName._runningLineCount, Variant.From<int>(ref _runningLineCount));
		info.AddProperty(PropertyName._completedLineCount, Variant.From<int>(ref _completedLineCount));
		info.AddProperty(PropertyName._forceCompleteRequested, Variant.From<bool>(ref _forceCompleteRequested));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		((GodotObject)this).RestoreGodotObjectData(info);
		Variant val = default(Variant);
		if (info.TryGetProperty(PropertyName._mat, ref val))
		{
			_mat = ((Variant)(ref val)).As<ShaderMaterial>();
		}
		Variant val2 = default(Variant);
		if (info.TryGetProperty(PropertyName._vpSize, ref val2))
		{
			_vpSize = ((Variant)(ref val2)).As<Vector2>();
		}
		Variant val3 = default(Variant);
		if (info.TryGetProperty(PropertyName._areaPos, ref val3))
		{
			_areaPos = ((Variant)(ref val3)).As<Vector2>();
		}
		Variant val4 = default(Variant);
		if (info.TryGetProperty(PropertyName._areaSize, ref val4))
		{
			_areaSize = ((Variant)(ref val4)).As<Vector2>();
		}
		Variant val5 = default(Variant);
		if (info.TryGetProperty(PropertyName._backBufferCopySibling, ref val5))
		{
			_backBufferCopySibling = ((Variant)(ref val5)).As<BackBufferCopy>();
		}
		Variant val6 = default(Variant);
		if (info.TryGetProperty(PropertyName._froms, ref val6))
		{
			_froms = ((Variant)(ref val6)).As<Vector2[]>();
		}
		Variant val7 = default(Variant);
		if (info.TryGetProperty(PropertyName._tos, ref val7))
		{
			_tos = ((Variant)(ref val7)).As<Vector2[]>();
		}
		Variant val8 = default(Variant);
		if (info.TryGetProperty(PropertyName._pendingTriggers, ref val8))
		{
			_pendingTriggers = ((Variant)(ref val8)).As<int>();
		}
		Variant val9 = default(Variant);
		if (info.TryGetProperty(PropertyName._nextTriggerSlot, ref val9))
		{
			_nextTriggerSlot = ((Variant)(ref val9)).As<int>();
		}
		Variant val10 = default(Variant);
		if (info.TryGetProperty(PropertyName._runningLineCount, ref val10))
		{
			_runningLineCount = ((Variant)(ref val10)).As<int>();
		}
		Variant val11 = default(Variant);
		if (info.TryGetProperty(PropertyName._completedLineCount, ref val11))
		{
			_completedLineCount = ((Variant)(ref val11)).As<int>();
		}
		Variant val12 = default(Variant);
		if (info.TryGetProperty(PropertyName._forceCompleteRequested, ref val12))
		{
			_forceCompleteRequested = ((Variant)(ref val12)).As<bool>();
		}
	}
}
