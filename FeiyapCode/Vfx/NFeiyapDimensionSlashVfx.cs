using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using Godot.Collections;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace Feiyap.Vfx;

[ScriptPath("res://Feiyap/Vfx/NFeiyapDimensionSlashVfx.cs")]
public partial class NFeiyapDimensionSlashVfx : ColorRect
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
				ResourceLoader.Load<PackedScene>(scenePath, cacheMode: ResourceLoader.CacheMode.Reuse);
			}
			catch (Exception ex)
			{
				Log.Warn("[Feiyap] NFeiyapDimensionSlashVfx 鍦烘櫙棰勭儹澶辫触 (" + scenePath + "): " + ex.Message, 2);
			}
		}
	}

	public static NFeiyapDimensionSlashVfx? Create(Vector2 areaGlobalPosition, Vector2 areaSize, SlashOptions options, Vector2[] froms, Vector2[] tos, Node? parentOverride = null)
	{
																if (string.IsNullOrEmpty(_slashScenePath))
		{
			Log.Warn("[Feiyap] NFeiyapDimensionSlashVfx 杩樻病 Initialize, 璺宠繃鐗规晥. 璇峰湪 mod 鍚姩鏃惰皟鐢?NFeiyapDimensionSlashVfx.Initialize(scenePath).", 2);
			return null;
		}
		NFeiyapDimensionSlashVfx slashVfx;
		try
		{
			slashVfx = PreloadManager.Cache.GetScene(_slashScenePath).Instantiate<NFeiyapDimensionSlashVfx>();
		}
		catch (Exception ex)
		{
			Log.Error("[Feiyap] 斩击特效场景实例化失败 (" + _slashScenePath + "): " + ex.Message, 2);
			return null;
		}
		Node parent = parentOverride ?? NCombatRoom.Instance;
		if (parent != null)
		{
			var backBuffer = new BackBufferCopy
			{
				CopyMode = BackBufferCopy.CopyModeEnum.Viewport,
				Name = "FeiyapSlashBackBufferCopy"
			};
			GodotTreeExtensions.AddChildSafely(parent, backBuffer);
			GodotTreeExtensions.AddChildSafely(parent, slashVfx);
			slashVfx._backBufferCopySibling = backBuffer;
			slashVfx.SetSlashArea(areaGlobalPosition, areaSize);
			slashVfx.SetSlashColor(Color.Color8(255, 81, 49, 255));
		}
		slashVfx.Options = options;
		slashVfx._froms = froms;
		slashVfx._tos = tos;
		return slashVfx;
	}

	public static NFeiyapDimensionSlashVfx? Create(NCreature nCreature, SlashOptions options, Vector2[] froms, Vector2[] tos)
	{
		if (string.IsNullOrEmpty(_slashScenePath))
		{
			Log.Warn("[Feiyap] 斩击特效尚未 Initialize，已跳过。", 2);
			return null;
		}

		NFeiyapDimensionSlashVfx slashVfx;
		try
		{
			slashVfx = PreloadManager.Cache.GetScene(_slashScenePath).Instantiate<NFeiyapDimensionSlashVfx>();
		}
		catch (Exception ex)
		{
			Log.Error("[Feiyap] 斩击特效场景实例化失败 (" + _slashScenePath + "): " + ex.Message, 2);
			return null;
		}

		var room = NCombatRoom.Instance;
		if (room != null)
		{
			var backBuffer = new BackBufferCopy
			{
				CopyMode = BackBufferCopy.CopyModeEnum.Viewport,
				Name = "FeiyapSlashBackBufferCopy"
			};
			GodotTreeExtensions.AddChildSafely(room, backBuffer);
			GodotTreeExtensions.AddChildSafely(room, slashVfx);
			slashVfx._backBufferCopySibling = backBuffer;
			slashVfx.SetSlashArea(nCreature.Hitbox.GlobalPosition, nCreature.Hitbox.Size);
			slashVfx.SetSlashColor(Color.Color8(255, 81, 49, 255));
		}

		slashVfx.Options = options;
		slashVfx._froms = froms;
		slashVfx._tos = tos;
		return slashVfx;
	}

	public static void GenerateRandomSlashLines(int count, float maxLength, float minLength, out Vector2[] froms, out Vector2[] tos, int seed = -1)
	{
																						count = Mathf.Max(0, count);
		minLength = Mathf.Clamp(minLength, 0f, 1f);
		maxLength = Mathf.Clamp(maxLength, 0f, 1f);
		if (minLength > maxLength)
		{
			float num = maxLength;
			maxLength = minLength;
			minLength = num;
		}
		froms = new Vector2[count];
		tos = new Vector2[count];
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
			val2 = new Vector2(val.RandfRange(num6, num7), val.RandfRange(num8, num9));
			Vector2 val3 = val2 + new Vector2(num4, num5);
			froms[i] = val2;
			tos[i] = val3;
		}
	}

	public static void GenerateConvergingSlashLines(int count, float length, float pivotSpread, out Vector2[] froms, out Vector2[] tos, int seed = -1)
	{
																																		count = Mathf.Max(0, count);
		froms = new Vector2[count];
		tos = new Vector2[count];
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
		val2 = new Vector2(0.5f, 0.5f);
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
			val4 = new Vector2(Mathf.Cos(num), Mathf.Sin(num));
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
																										Material material = this.Material;
		ShaderMaterial val = (ShaderMaterial)(object)((material is ShaderMaterial) ? material : null);
		if (val != null)
		{
			_mat = (ShaderMaterial)val.Duplicate(false);
			this.Material = (Material)(object)_mat;
		}
		else if (!_matFailureReported)
		{
			_matFailureReported = true;
			Log.Warn("[Feiyap] NFeiyapDimensionSlashVfx 鍦烘櫙缂哄皯 ShaderMaterial, 鐗规晥涓嶄細鏄剧ず. (鍚庣画鐩稿悓閿欒灏嗛潤榛?", 2);
		}
		Rect2 visibleRect = this.GetViewport().GetVisibleRect();
		_vpSize = visibleRect.Size;
		_areaPos = Vector2.Zero;
		_areaSize = _vpSize;
		this.GlobalPosition = Vector2.Zero;
		this.Size = _vpSize;
		for (int i = 0; i < 64; i++)
		{
			_lineProgresses[i] = 0.5f;
			_lineAlphas[i] = 0f;
		}
		this.GetViewport().SizeChanged += delegate
		{
			var visibleRect2 = this.GetViewport().GetVisibleRect();
			_vpSize = visibleRect2.Size;
			this.GlobalPosition = Vector2.Zero;
			this.Size = _vpSize;
		};
		this.TreeExited += delegate
		{
			if (_backBufferCopySibling != null && GodotObject.IsInstanceValid(_backBufferCopySibling))
			{
				_backBufferCopySibling.QueueFree();
			}
		};
	}

	public void SetSlashColor(Color color)
	{
						ShaderMaterial? mat = _mat;
		if (mat != null)
		{
			mat.SetShaderParameter("glow_color", color);
		}
	}

	public void SetSlashArea(Vector2 globalPosition, Vector2 size)
	{
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
		this.QueueFree();
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
		_mat.SetShaderParameter("slash_progress", 0f);
		FlushLines();
		Tween val = this.CreateTween().SetParallel(true);
		val.TweenMethod(Callable.From<float>((Action<float>)delegate(float v)
		{
			for (int j = 0; j < count; j++)
			{
				_lineProgresses[j] = v;
			}
			FlushLines();
		}), 0f, 0.5f, (double)Options.ExpandSlashDuration);
		val.TweenMethod(Callable.From<float>((Action<float>)delegate(float v)
		{
						_mat.SetShaderParameter("slash_progress", v);
		}), 0f, 1f, (double)Options.ExpandSlashDuration);
		float num = Mathf.Max(0.0001f, Options.LineFadeInDuration);
		val.TweenMethod(Callable.From<float>((Action<float>)delegate(float v)
		{
			for (int j = 0; j < count; j++)
			{
				_lineAlphas[j] = v;
			}
			FlushLines();
		}), 0f, 1f, (double)num);
		await ToSignal(val, Tween.SignalName.Finished);
		await ToSignal(GetTree().CreateTimer(Options.KeepSlashDuration), SceneTreeTimer.SignalName.Timeout);
		Tween val2 = this.CreateTween();
		val2.TweenMethod(Callable.From<float>((Action<float>)delegate(float v)
		{
						_mat.SetShaderParameter("slash_progress", v);
		}), 1f, 0f, (double)Options.ContractSlashDuration);
		await ToSignal(val2, Tween.SignalName.Finished);
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
		_mat.SetShaderParameter("slash_progress", 0f);
		for (int i = 0; i < total; i++)
		{
			_lineActive[i] = true;
			_lineProgresses[i] = 0f;
			_lineAlphas[i] = 0f;
			int captured = i;
			Tween val = this.CreateTween().SetParallel(true);
			val.TweenMethod(Callable.From<float>((Action<float>)delegate(float v)
			{
				_lineProgresses[captured] = v;
				FlushLines();
			}), 0f, 0.5f, (double)Options.ExpandSlashDuration);
			val.TweenMethod(Callable.From<float>((Action<float>)delegate(float v)
			{
				_lineAlphas[captured] = v;
				FlushLines();
			}), 0f, 1f, (double)Mathf.Max(0.0001f, Options.LineFadeInDuration));
			if (i == 0)
			{
				val.TweenMethod(Callable.From<float>((Action<float>)delegate(float v)
				{
										_mat.SetShaderParameter("slash_progress", v);
				}), 0f, 1f, (double)Options.ExpandSlashDuration);
			}
			await ToSignal(val, Tween.SignalName.Finished);
			if (i < total - 1)
			{
				await ToSignal(GetTree().CreateTimer(Options.EachSlashShownInterval), SceneTreeTimer.SignalName.Timeout);
			}
		}
		await ToSignal(GetTree().CreateTimer(Options.KeepSlashDuration), SceneTreeTimer.SignalName.Timeout);
		Tween val2 = this.CreateTween();
		val2.TweenMethod(Callable.From<float>((Action<float>)delegate(float v)
		{
						_mat.SetShaderParameter("slash_progress", v);
		}), 1f, 0f, (double)Options.ContractSlashDuration);
		await ToSignal(val2, Tween.SignalName.Finished);
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
		_mat.SetShaderParameter("slash_progress", 0f);
		for (int i = 0; i < total; i++)
		{
			await ConsumeOneTrigger();
			_nextTriggerSlot = i + 1;
			_lineActive[i] = true;
			_lineProgresses[i] = 0f;
			_lineAlphas[i] = 0f;
			int captured = i;
			Tween val = this.CreateTween().SetParallel(true);
			val.TweenMethod(Callable.From<float>((Action<float>)delegate(float v)
			{
				_lineProgresses[captured] = v;
				FlushLines();
			}), 0f, 0.5f, (double)Options.ExpandSlashDuration);
			val.TweenMethod(Callable.From<float>((Action<float>)delegate(float v)
			{
				_lineAlphas[captured] = v;
				FlushLines();
			}), 0f, 1f, (double)Mathf.Max(0.0001f, Options.LineFadeInDuration));
			if (i == 0)
			{
				val.TweenMethod(Callable.From<float>((Action<float>)delegate(float v)
				{
										_mat.SetShaderParameter("slash_progress", v);
				}), 0f, 1f, (double)Options.ExpandSlashDuration);
			}
			await ToSignal(val, Tween.SignalName.Finished);
		}
		await ToSignal(GetTree().CreateTimer(Options.KeepSlashDuration), SceneTreeTimer.SignalName.Timeout);
		Tween val2 = this.CreateTween();
		val2.TweenMethod(Callable.From<float>((Action<float>)delegate(float v)
		{
						_mat.SetShaderParameter("slash_progress", v);
		}), 1f, 0f, (double)Options.ContractSlashDuration);
		await ToSignal(val2, Tween.SignalName.Finished);
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
			_mat.SetShaderParameter("slash_progress", 0f);
			await _allDoneTcs.Task;
			Tween val = this.CreateTween();
			val.TweenMethod(Callable.From<float>((Action<float>)delegate(float v)
			{
								_mat.SetShaderParameter("slash_progress", v);
			}), 1f, 0f, (double)Options.ContractSlashDuration);
			await ToSignal(val, Tween.SignalName.Finished);
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
		Tween val = this.CreateTween().SetParallel(true);
		val.TweenMethod(Callable.From<float>((Action<float>)delegate(float v)
		{
			_lineProgresses[slot] = v;
			FlushLines();
		}), 0f, 0.5f, (double)Options.ExpandSlashDuration);
		val.TweenMethod(Callable.From<float>((Action<float>)delegate(float v)
		{
			_lineAlphas[slot] = v;
			FlushLines();
		}), 0f, 1f, (double)Mathf.Max(0.0001f, Options.LineFadeInDuration));
		if (num)
		{
			val.TweenMethod(Callable.From<float>((Action<float>)delegate(float v)
			{
								_mat.SetShaderParameter("slash_progress", v);
			}), 0f, 1f, (double)Options.ExpandSlashDuration);
		}
		await ToSignal(val, Tween.SignalName.Finished);
		await ToSignal(GetTree().CreateTimer(Options.KeepSlashDuration), SceneTreeTimer.SignalName.Timeout);
		Tween val2 = this.CreateTween();
		val2.TweenMethod(Callable.From<float>((Action<float>)delegate(float v)
		{
			_lineAlphas[slot] = v;
			FlushLines();
		}), 1f, 0f, (double)Options.ContractSlashDuration);
		await ToSignal(val2, Tween.SignalName.Finished);
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
																return (_areaPos + areaUV * _areaSize) / _vpSize;
	}

	private void FlushLines()
	{
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
		_mat.SetShaderParameter("slash_points_a", val);
		_mat.SetShaderParameter("slash_points_b", val2);
		_mat.SetShaderParameter("slash_weights", val3);
		_mat.SetShaderParameter("slash_line_progress", val4);
		int num = ((_froms != null) ? Mathf.Min(_froms.Length, 64) : 0);
		_mat.SetShaderParameter("slash_count", num);
	}
}
