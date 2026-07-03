using System;
using Danjin.Character;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Vfx.Utilities;

namespace Danjin.Patches;

[HarmonyPatch(typeof(NEnergyCounter), "_Ready")]
internal static class NEnergyCounterDanjinVfxPatch
{
	private const string BackVfxPath = "res://scenes/vfx/energy/defect/defect_energy_vfx_back.tscn";

	private const string FrontVfxPath = "res://scenes/vfx/energy/defect/defect_energy_vfx_front.tscn";

	[HarmonyPrefix]
	private static void Prefix(NEnergyCounter __instance)
	{
		try
		{
			object? value = AccessTools.Field(typeof(NEnergyCounter), "_player").GetValue(__instance);
			object? obj = ((value is Player) ? value : null);
			if (((obj != null) ? ((Player)obj).Character : null) is Danjin.Character.Danjin)
			{
				ReplacePlaceholder(__instance, "EnergyVfxBack", "res://scenes/vfx/energy/defect/defect_energy_vfx_back.tscn");
				ReplacePlaceholder(__instance, "EnergyVfxFront", "res://scenes/vfx/energy/defect/defect_energy_vfx_front.tscn");
			}
		}
		catch (Exception value2)
		{
			Log.Error($">>>[DanjinMod] 能量盘特效注入失败: {value2}", 1);
		}
	}

	private static void ReplacePlaceholder(NEnergyCounter counter, string nodeName, string scenePath)
	{
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		Node nodeOrNull = ((Node)counter).GetNodeOrNull(NodePath.op_Implicit(nodeName));
		if (nodeOrNull == null)
		{
			Log.Warn("[Danjin] 能量盘找不到占位节点 " + nodeName + "，跳过特效注入", 2);
			return;
		}
		PackedScene val = ResourceLoader.Load<PackedScene>(scenePath, (string)null, (CacheMode)1);
		if (val == null)
		{
			Log.Warn("[Danjin] 能量盘特效场景加载失败: " + scenePath, 2);
			return;
		}
		NParticlesContainer val2 = val.Instantiate<NParticlesContainer>((GenEditState)0);
		((Node2D)val2).Position = new Vector2(64f, 54f);
		int index = nodeOrNull.GetIndex(false);
		Node parent = nodeOrNull.GetParent();
		nodeOrNull.Name = StringName.op_Implicit(nodeName + "_OldPlaceholder");
		parent.RemoveChild(nodeOrNull);
		nodeOrNull.QueueFree();
		((Node)val2).Name = StringName.op_Implicit(nodeName);
		parent.AddChild((Node)(object)val2, false, (InternalMode)0);
		parent.MoveChild((Node)(object)val2, index);
		((Node)val2).Owner = (Node)(object)counter;
		((Node)val2).UniqueNameInOwner = true;
	}
}
