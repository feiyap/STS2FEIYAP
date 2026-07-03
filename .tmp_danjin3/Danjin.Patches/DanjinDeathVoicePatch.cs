using System;
using System.Collections;
using System.Reflection;
using Danjin.Audio;
using Danjin.Character;
using Danjin.Utils;
using HarmonyLib;

namespace Danjin.Patches;

[HarmonyPatch]
internal class DanjinDeathVoicePatch
{
	private static ModSound? _death1;

	private static ModSound? _death2;

	private static readonly Random _rng = new Random();

	private static MethodInfo TargetMethod()
	{
		return AccessTools.Method(AccessTools.TypeByName("MegaCrit.Sts2.Core.Nodes.Screens.GameOverScreen.NGameOverScreen"), "_Ready", (Type[])null, (Type[])null);
	}

	[HarmonyPostfix]
	private static void Postfix(object __instance)
	{
		try
		{
			object obj = AccessTools.Field(__instance.GetType(), "_runState")?.GetValue(__instance);
			if (obj == null)
			{
				return;
			}
			object obj2 = AccessTools.Property(obj.GetType(), "CurrentRoom")?.GetValue(obj);
			if (obj2 != null)
			{
				object obj3 = AccessTools.Property(obj2.GetType(), "IsVictoryRoom")?.GetValue(obj2);
				if (obj3 is bool && (bool)obj3)
				{
					return;
				}
			}
			PropertyInfo propertyInfo = AccessTools.Property(obj.GetType(), "Players");
			if (!(propertyInfo == null) && propertyInfo.GetValue(obj) is IList { Count: not 0 } list && AccessTools.Property(list[0].GetType(), "Character")?.GetValue(list[0]) is Danjin.Character.Danjin)
			{
				if (_death1 == null)
				{
					_death1 = new ModSound("res://Danjin/Sounds/danjin_death_1.ogg");
				}
				if (_death2 == null)
				{
					_death2 = new ModSound("res://Danjin/Sounds/danjin_death_2.ogg");
				}
				DanjinVoice.Play((_rng.Next(2) == 0) ? _death1 : _death2);
			}
		}
		catch
		{
		}
	}
}
