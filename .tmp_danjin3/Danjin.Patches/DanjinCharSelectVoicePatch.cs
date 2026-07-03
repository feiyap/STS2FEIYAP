using System;
using System.Reflection;
using Danjin.Audio;
using Danjin.Character;
using Danjin.Utils;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;

namespace Danjin.Patches;

[HarmonyPatch]
internal class DanjinCharSelectVoicePatch
{
	private static ModSound? _select1;

	private static ModSound? _select2;

	private static int _lastSelect = -1;

	private static readonly Random _rng = new Random();

	private static MethodInfo TargetMethod()
	{
		return AccessTools.Method(AccessTools.TypeByName("MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect.NCharacterSelectScreen"), "SelectCharacter", (Type[])null, (Type[])null);
	}

	[HarmonyPostfix]
	private static void Postfix(object __instance, CharacterModel characterModel)
	{
		if (characterModel is Danjin.Character.Danjin)
		{
			if (_select1 == null)
			{
				_select1 = new ModSound("res://Danjin/Sounds/danjin_select_1.ogg");
			}
			if (_select2 == null)
			{
				_select2 = new ModSound("res://Danjin/Sounds/danjin_select_2.ogg");
			}
			int num = _lastSelect switch
			{
				0 => 1, 
				1 => 0, 
				_ => _rng.Next(2), 
			};
			DanjinVoice.Play((num == 0) ? _select1 : _select2);
			_lastSelect = num;
		}
	}
}
