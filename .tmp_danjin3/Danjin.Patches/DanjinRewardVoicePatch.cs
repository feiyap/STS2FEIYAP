using System;
using System.Collections;
using System.Reflection;
using Danjin.Audio;
using Danjin.Character;
using Danjin.Utils;
using HarmonyLib;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;

namespace Danjin.Patches;

[HarmonyPatch]
internal class DanjinRewardVoicePatch
{
	private static ModSound?[] _combatSounds = new ModSound[3];

	private static ModSound?[] _eventSounds = new ModSound[2];

	private static int _lastCombat = -1;

	private static readonly Random _rng = new Random();

	private static MethodInfo TargetMethod()
	{
		return AccessTools.Method(AccessTools.TypeByName("MegaCrit.Sts2.Core.Nodes.Screens.NRewardsScreen"), "_Ready", (Type[])null, (Type[])null);
	}

	[HarmonyPostfix]
	private static void Postfix(object __instance)
	{
		try
		{
			RunManager instance = RunManager.Instance;
			object obj = AccessTools.Property(((object)instance).GetType(), "State")?.GetValue(instance);
			if (obj == null || !(AccessTools.Property(obj.GetType(), "Players")?.GetValue(obj) is IList { Count: not 0 } list) || !(AccessTools.Property(list[0].GetType(), "Character")?.GetValue(list[0]) is Danjin.Character.Danjin))
			{
				return;
			}
			object obj2 = AccessTools.Property(obj.GetType(), "CurrentRoom")?.GetValue(obj);
			if (obj2 is CombatRoom)
			{
				for (int i = 0; i < 3; i++)
				{
					ref ModSound reference = ref _combatSounds[i];
					if (reference == null)
					{
						reference = new ModSound($"res://Danjin/Sounds/danjin_reward_{i + 1}.ogg");
					}
				}
				int num;
				do
				{
					num = _rng.Next(3);
				}
				while (num == _lastCombat);
				_lastCombat = num;
				DanjinVoice.Play(_combatSounds[num]);
			}
			else
			{
				if (!(obj2 is EventRoom))
				{
					return;
				}
				for (int j = 0; j < 2; j++)
				{
					ref ModSound reference = ref _eventSounds[j];
					if (reference == null)
					{
						reference = new ModSound($"res://Danjin/Sounds/danjin_event_reward_{j + 1}.ogg");
					}
				}
				DanjinVoice.Play(_eventSounds[_rng.Next(2)]);
			}
		}
		catch
		{
		}
	}
}
