using System;
using System.Reflection;
using Danjin.Audio;
using Danjin.Character;
using Danjin.Utils;
using HarmonyLib;

namespace Danjin.Patches;

[HarmonyPatch]
internal class DanjinEmbarkVoicePatch
{
	private static ModSound? _start1;

	private static ModSound? _start2;

	private static readonly Random _rng = new Random();

	private static MethodInfo TargetMethod()
	{
		return AccessTools.Method(AccessTools.TypeByName("MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect.NCharacterSelectScreen"), "OnEmbarkPressed", (Type[])null, (Type[])null);
	}

	[HarmonyPostfix]
	private static void Postfix(object __instance)
	{
		try
		{
			FieldInfo fieldInfo = AccessTools.Field(__instance.GetType(), "_selectedButton");
			if (fieldInfo == null)
			{
				return;
			}
			object value = fieldInfo.GetValue(__instance);
			if (value == null)
			{
				return;
			}
			PropertyInfo propertyInfo = AccessTools.Property(value.GetType(), "Character");
			if (!(propertyInfo == null) && /*isinst with value type is only supported in some contexts*/is Danjin.Character.Danjin)
			{
				if (_start1 == null)
				{
					_start1 = new ModSound("res://Danjin/Sounds/danjin_start_1.ogg");
				}
				if (_start2 == null)
				{
					_start2 = new ModSound("res://Danjin/Sounds/danjin_start_2.ogg");
				}
				DanjinVoice.Play((_rng.Next(2) == 0) ? _start1 : _start2);
			}
		}
		catch
		{
		}
	}
}
