using System;
using System.Collections;
using System.Reflection;
using Danjin.Audio;
using Danjin.Character;
using Danjin.Utils;
using HarmonyLib;
using MegaCrit.Sts2.Core.Runs;

namespace Danjin.Patches;

[HarmonyPatch]
internal class DanjinTreasureRoomVoicePatch
{
	private static ModSound? _treasure1;

	private static ModSound? _treasure2;

	private static readonly Random _rng = new Random();

	private static MethodInfo TargetMethod()
	{
		return AccessTools.Method(AccessTools.TypeByName("MegaCrit.Sts2.Core.Nodes.Rooms.NTreasureRoom"), "_Ready", (Type[])null, (Type[])null);
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
			if (propertyInfo == null || !(propertyInfo.GetValue(obj) is IList { Count: not 0 } list))
			{
				return;
			}
			PropertyInfo propertyInfo2 = AccessTools.Property(list[0].GetType(), "Character");
			if (!(propertyInfo2 == null) && propertyInfo2.GetValue(list[0]) is Danjin.Character.Danjin)
			{
				if (_treasure1 == null)
				{
					_treasure1 = new ModSound("res://Danjin/Sounds/danjin_treasure_1.ogg");
				}
				if (_treasure2 == null)
				{
					_treasure2 = new ModSound("res://Danjin/Sounds/danjin_treasure_2.ogg");
				}
				DanjinVoice.Play((_rng.Next(2) == 0) ? _treasure1 : _treasure2);
			}
		}
		catch
		{
		}
	}
}
