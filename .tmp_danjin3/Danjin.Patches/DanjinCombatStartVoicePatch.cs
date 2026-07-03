using System;
using System.Reflection;
using Danjin.Audio;
using Danjin.Character;
using Danjin.Utils;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Runs;

namespace Danjin.Patches;

[HarmonyPatch]
internal class DanjinCombatStartVoicePatch
{
	internal static bool _afterRestSite;

	private static ModSound? _combatStart;

	private static MethodInfo TargetMethod()
	{
		return AccessTools.Method(AccessTools.TypeByName("MegaCrit.Sts2.Core.Hooks.Hook"), "BeforeCombatStart", (Type[])null, (Type[])null);
	}

	[HarmonyPostfix]
	private static void Postfix(IRunState runState)
	{
		try
		{
			if (!_afterRestSite)
			{
				return;
			}
			_afterRestSite = false;
			if (((runState != null) ? ((IPlayerCollection)runState).Players : null) == null || ((IPlayerCollection)runState).Players.Count == 0)
			{
				return;
			}
			Player obj = ((IPlayerCollection)runState).Players[0];
			if (((obj != null) ? obj.Character : null) is Danjin.Character.Danjin)
			{
				if (_combatStart == null)
				{
					_combatStart = new ModSound("res://Danjin/Sounds/danjin_combat_start.ogg");
				}
				DanjinVoice.Play(_combatStart);
			}
		}
		catch
		{
		}
	}
}
