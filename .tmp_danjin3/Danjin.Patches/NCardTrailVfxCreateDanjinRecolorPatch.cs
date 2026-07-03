using System;
using Danjin.Character;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Runs;

namespace Danjin.Patches;

[HarmonyPatch(typeof(NCardTrailVfx), "Create")]
internal static class NCardTrailVfxCreateDanjinRecolorPatch
{
	[HarmonyPrefix]
	private static void Prefix(out bool __state)
	{
		__state = IsLocalPlayerDanjin();
	}

	[HarmonyPostfix]
	private static void Postfix(NCardTrailVfx? __result, bool __state)
	{
		if (!__state || __result == null)
		{
			return;
		}
		try
		{
			DanjinTrailRecolor.Apply(__result);
		}
		catch (Exception value)
		{
			Log.Error($">>>[DanjinMod] 卡牌拖尾绯红化染色失败: {value}", 1);
		}
	}

	private static bool IsLocalPlayerDanjin()
	{
		try
		{
			RunManager instance = RunManager.Instance;
			RunState val = ((instance != null) ? instance.DebugOnlyGetState() : null);
			if (val != null)
			{
				Player me = LocalContext.GetMe((IPlayerCollection)(object)val);
				if (((me != null) ? me.Character : null) is Danjin.Character.Danjin)
				{
					return true;
				}
				return false;
			}
			CombatManager instance2 = CombatManager.Instance;
			if (instance2 == null || !instance2.IsInProgress)
			{
				return false;
			}
			CombatState val2 = instance2.DebugOnlyGetState();
			if (val2 == null)
			{
				return false;
			}
			Player me2 = LocalContext.GetMe((ICombatState)(object)val2);
			return ((me2 != null) ? me2.Character : null) is Danjin.Character.Danjin;
		}
		catch
		{
			return false;
		}
	}
}
