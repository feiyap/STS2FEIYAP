using System;
using System.Reflection;
using HarmonyLib;

namespace Danjin.Patches;

[HarmonyPatch]
internal class DanjinHurtVoiceTurnReset
{
	internal static int TurnId;

	private static MethodInfo TargetMethod()
	{
		return AccessTools.Method(AccessTools.TypeByName("MegaCrit.Sts2.Core.Hooks.Hook"), "BeforeSideTurnStart", (Type[])null, (Type[])null);
	}

	[HarmonyPostfix]
	private static void Postfix()
	{
		TurnId++;
	}
}
