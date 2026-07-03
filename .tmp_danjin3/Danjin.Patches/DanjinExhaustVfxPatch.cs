using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Danjin.Character;
using Danjin.Vfx;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Vfx.Cards;

namespace Danjin.Patches;

[HarmonyPatch(typeof(NExhaustVfx), "_Ready")]
internal static class DanjinExhaustVfxPatch
{
	private const float HoldVisibleDuration = 0.05f;

	private const float CardFadeDuration = 0.13f;

	private const float SlashExpand = 0.06f;

	private const float SlashKeep = 0.04f;

	private const float SlashContract = 0.08f;

	private const float SlashFadeIn = 0.03f;

	private static readonly Vector2[] SlashFroms = (Vector2[])(object)new Vector2[1]
	{
		new Vector2(1.1f, -0.1f)
	};

	private static readonly Vector2[] SlashTos = (Vector2[])(object)new Vector2[1]
	{
		new Vector2(-0.1f, 1.1f)
	};

	private static readonly Color SlashGlowColor = new Color(0.96f, 0.1f, 0.22f, 1f);

	private const float CutStrengthBoost = 0.08f;

	private const float CutOuterRatio = 0.03f;

	private const float CutFarRatio = 0.07f;

	private static readonly Vector2 FallbackCardSize = new Vector2(220f, 308f);

	private static readonly FieldInfo? _cardField = AccessTools.Field(typeof(NExhaustVfx), "_card");

	[HarmonyPostfix]
	private static void Postfix(NExhaustVfx __instance)
	{
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			if (_cardField == null)
			{
				Log.Warn("[Danjin] NExhaustVfx._card 反射失败", 2);
				return;
			}
			object? value = _cardField.GetValue(__instance);
			NCard val = (NCard)((value is NCard) ? value : null);
			if (val == null)
			{
				return;
			}
			CardModel model = val.Model;
			object obj;
			if (model == null)
			{
				obj = null;
			}
			else
			{
				Player owner = model.Owner;
				obj = ((owner != null) ? owner.Character : null);
			}
			if (!(obj is Danjin.Character.Danjin) || !((CanvasItem)__instance).Visible)
			{
				return;
			}
			int num = 0;
			foreach (GpuParticles2D item in ((IEnumerable)((Node)__instance).GetChildren(false)).OfType<GpuParticles2D>())
			{
				item.Emitting = false;
				((CanvasItem)item).Visible = false;
				num++;
			}
			string source;
			Rect2 rect = ComputeCardRect(val, out source);
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(54, 5);
			defaultInterpolatedStringHandler.AppendLiteral("[Danjin消耗特效] card=");
			CardModel model2 = val.Model;
			defaultInterpolatedStringHandler.AppendFormatted((model2 != null) ? ((AbstractModel)model2).Id.Entry : null);
			defaultInterpolatedStringHandler.AppendLiteral(" pos=");
			defaultInterpolatedStringHandler.AppendFormatted<Vector2>(((Rect2)(ref rect)).Position);
			defaultInterpolatedStringHandler.AppendLiteral(" size=");
			defaultInterpolatedStringHandler.AppendFormatted<Vector2>(((Rect2)(ref rect)).Size);
			defaultInterpolatedStringHandler.AppendLiteral(" source=");
			defaultInterpolatedStringHandler.AppendFormatted(source);
			defaultInterpolatedStringHandler.AppendLiteral(" killedParticles=");
			defaultInterpolatedStringHandler.AppendFormatted(num);
			Log.Info(defaultInterpolatedStringHandler.ToStringAndClear(), 2);
			if (((Rect2)(ref rect)).Size.X < 1f || ((Rect2)(ref rect)).Size.Y < 1f)
			{
				Log.Warn("[Danjin消耗特效] 卡牌矩形无效, 跳过", 2);
				return;
			}
			TakeOverCardFade(val);
			TrySpawnSlash(rect);
		}
		catch (Exception value2)
		{
			Log.Error($"[Danjin消耗特效] 顶层异常: {value2}", 2);
		}
	}

	private static void TakeOverCardFade(NCard card)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		Tween tween;
		try
		{
			((CanvasItem)card).Modulate = Colors.White;
			tween = ((Node)card).CreateTween();
			tween.SetTrans((TransitionType)7).SetEase((EaseType)1);
			tween.TweenProperty((GodotObject)(object)card, NodePath.op_Implicit("modulate"), Variant.op_Implicit(Colors.White), 0.05000000074505806);
			tween.TweenProperty((GodotObject)(object)card, NodePath.op_Implicit("modulate"), Variant.op_Implicit(new Color(1f, 1f, 1f, 0f)), 0.12999999523162842);
			((Node)card).TreeExiting += KillTweenOnExit;
		}
		catch (Exception ex)
		{
			Log.Warn("[Danjin消耗特效] 接管卡牌淡出失败: " + ex.Message, 2);
		}
		void KillTweenOnExit()
		{
			if (tween.IsValid())
			{
				tween.Kill();
			}
			((Node)card).TreeExiting -= KillTweenOnExit;
		}
	}

	private static void TrySpawnSlash(Rect2 rect)
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			NDimensionSlashVfx.SlashOptions slashOptions = new NDimensionSlashVfx.SlashOptions();
			slashOptions.Mode = 0;
			slashOptions.ExpandSlashDuration = 0.06f;
			slashOptions.KeepSlashDuration = 0.04f;
			slashOptions.ContractSlashDuration = 0.08f;
			slashOptions.LineFadeInDuration = 0.03f;
			NDimensionSlashVfx.SlashOptions options = slashOptions;
			NDimensionSlashVfx nDimensionSlashVfx = NDimensionSlashVfx.Create(((Rect2)(ref rect)).Position, ((Rect2)(ref rect)).Size, options, SlashFroms, SlashTos);
			if (nDimensionSlashVfx == null)
			{
				Log.Warn("[Danjin消耗特效] slash Create 返回 null", 2);
				return;
			}
			nDimensionSlashVfx.SetSlashColor(SlashGlowColor);
			Material material = ((CanvasItem)nDimensionSlashVfx).Material;
			ShaderMaterial val = (ShaderMaterial)(object)((material is ShaderMaterial) ? material : null);
			if (val != null)
			{
				val.SetShaderParameter(StringName.op_Implicit("cut_strength"), Variant.op_Implicit(0.08f));
				val.SetShaderParameter(StringName.op_Implicit("cut_outer_ratio"), Variant.op_Implicit(0.03f));
				val.SetShaderParameter(StringName.op_Implicit("cut_far_ratio"), Variant.op_Implicit(0.07f));
			}
			nDimensionSlashVfx.TriggerSlash();
		}
		catch (Exception ex)
		{
			Log.Warn("[Danjin消耗特效] slash 生成失败: " + ex.Message, 2);
		}
	}

	private static Rect2 ComputeCardRect(NCard card, out string source)
	{
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			Control body = card.Body;
			if (body != null)
			{
				Rect2 globalRect = body.GetGlobalRect();
				if (((Rect2)(ref globalRect)).Size.X > 1f && ((Rect2)(ref globalRect)).Size.Y > 1f)
				{
					source = "Body";
					return globalRect;
				}
			}
		}
		catch
		{
		}
		try
		{
			Rect2 globalRect2 = ((Control)card).GetGlobalRect();
			if (((Rect2)(ref globalRect2)).Size.X > 1f && ((Rect2)(ref globalRect2)).Size.Y > 1f)
			{
				source = "Card";
				return globalRect2;
			}
		}
		catch
		{
		}
		try
		{
			source = "Fallback";
			return new Rect2(((Control)card).GlobalPosition - FallbackCardSize * 0.5f, FallbackCardSize);
		}
		catch
		{
			source = "ZeroFallback";
			return new Rect2(Vector2.Zero, FallbackCardSize);
		}
	}
}
