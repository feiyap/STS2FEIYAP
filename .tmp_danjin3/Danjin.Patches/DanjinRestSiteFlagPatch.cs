using System;
using System.Collections;
using System.Reflection;
using Danjin.Character;
using HarmonyLib;
using MegaCrit.Sts2.Core.Runs;

namespace Danjin.Patches;

[HarmonyPatch]
internal class DanjinRestSiteFlagPatch
{
	private static MethodInfo TargetMethod()
	{
		return AccessTools.Method(AccessTools.TypeByName("MegaCrit.Sts2.Core.Nodes.Rooms.NRestSiteRoom"), "_Ready", (Type[])null, (Type[])null);
	}

	[HarmonyPostfix]
	private static void Postfix()
	{
		try
		{
			RunManager instance = RunManager.Instance;
			object obj = AccessTools.Property(((object)instance).GetType(), "State")?.GetValue(instance);
			if (obj == null)
			{
				return;
			}
			PropertyInfo propertyInfo = AccessTools.Property(obj.GetType(), "Players");
			if (!(propertyInfo == null) && propertyInfo.GetValue(obj) is IList { Count: not 0 } list)
			{
				PropertyInfo propertyInfo2 = AccessTools.Property(list[0].GetType(), "Character");
				if (!(propertyInfo2 == null) && propertyInfo2.GetValue(list[0]) is Danjin.Character.Danjin)
				{
					DanjinCombatStartVoicePatch._afterRestSite = true;
				}
			}
		}
		catch
		{
		}
	}
}
