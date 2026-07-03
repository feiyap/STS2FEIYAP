using System;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Nodes.Vfx.Utilities;

namespace Danjin.Patches;

[HarmonyPatch(typeof(NStarCounter), "UpdateStarCount")]
internal static class TonghuaGainVfxPatch
{
	private static readonly Color TonghuaBurstTint = new Color(0.82f, 0.12f, 0.42f, 1f);

	private const float BurstScale = 0.24f;

	[HarmonyPostfix]
	private static void Postfix(NStarCounter __instance, int oldCount, int newCount)
	{
		if (!TonghuaUiBindingPatch.IsTonghuaCounter(__instance))
		{
			return;
		}
		try
		{
			foreach (Node child in ((Node)__instance).GetChildren(false))
			{
				if (child is NVfxParticleSystem)
				{
					child.QueueFree();
					break;
				}
			}
			if (newCount > oldCount)
			{
				SpawnFireBurst(__instance);
			}
		}
		catch (Exception value)
		{
			Log.Error($"[Danjin] 彤华 counter 获得特效处理失败: {value}", 2);
		}
	}

	private static void SpawnFireBurst(NStarCounter counter)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		Vector2 val = new Vector2(((Control)counter).Size.X * 0.5f, ((Control)counter).Size.Y * 0.72f) * ((Control)counter).Scale;
		Vector2 val2 = ((Control)counter).GlobalPosition + val;
		NFireBurstVfx val3 = NFireBurstVfx.Create(val2, 0.24f, TonghuaBurstTint);
		if (val3 != null)
		{
			((Node)counter).AddChild((Node)(object)val3, false, (InternalMode)0);
			((Node2D)val3).GlobalPosition = val2;
		}
	}
}
