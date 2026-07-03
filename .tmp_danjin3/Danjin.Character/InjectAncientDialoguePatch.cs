using System.Collections.Generic;
using HarmonyLib;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;

namespace Danjin.Character;

[HarmonyPatch(typeof(ModelDb), "Init")]
internal class InjectAncientDialoguePatch
{
	[HarmonyPostfix]
	private static void Inject()
	{
		LocTable table = LocManager.Instance.GetTable("ancients");
		if (AccessTools.Field(typeof(LocTable), "_translations").GetValue(table) is Dictionary<string, string> dictionary)
		{
			dictionary["THE_ARCHITECT.talk.DANJIN_CHARACTER_DANJIN.0-0.ancient"] = "又一个……闯入者。你不该来这里。";
			dictionary["THE_ARCHITECT.talk.DANJIN_CHARACTER_DANJIN.0-0.sfx"] = "";
			dictionary["THE_ARCHITECT.talk.DANJIN_CHARACTER_DANJIN.0-0.next"] = "……";
			dictionary["THE_ARCHITECT.talk.DANJIN_CHARACTER_DANJIN.0-1.char"] = "我为枉死的人们而来。";
			dictionary["THE_ARCHITECT.talk.DANJIN_CHARACTER_DANJIN.0-1.sfx"] = "";
			dictionary["THE_ARCHITECT.talk.DANJIN_CHARACTER_DANJIN.0-attack"] = "Both";
			dictionary["THE_ARCHITECT.talk.DANJIN_CHARACTER_DANJIN.1-0.ancient"] = "你不该干涉此事。秩序不容扰乱。";
			dictionary["THE_ARCHITECT.talk.DANJIN_CHARACTER_DANJIN.1-0.sfx"] = "";
			dictionary["THE_ARCHITECT.talk.DANJIN_CHARACTER_DANJIN.1-0.next"] = "……";
			dictionary["THE_ARCHITECT.talk.DANJIN_CHARACTER_DANJIN.1-1.char"] = "杀人偿命，天经地义。";
			dictionary["THE_ARCHITECT.talk.DANJIN_CHARACTER_DANJIN.1-1.sfx"] = "";
			dictionary["THE_ARCHITECT.talk.DANJIN_CHARACTER_DANJIN.1-attack"] = "Both";
			dictionary["THE_ARCHITECT.talk.DANJIN_CHARACTER_DANJIN.2-0r.ancient"] = "你的执念,倒比刀刃更利。";
			dictionary["THE_ARCHITECT.talk.DANJIN_CHARACTER_DANJIN.2-0r.sfx"] = "";
			dictionary["THE_ARCHITECT.talk.DANJIN_CHARACTER_DANJIN.2-0r.next"] = "……";
			dictionary["THE_ARCHITECT.talk.DANJIN_CHARACTER_DANJIN.2-1r.char"] = "多说无益。贼寇受死！";
			dictionary["THE_ARCHITECT.talk.DANJIN_CHARACTER_DANJIN.2-1r.sfx"] = "";
			dictionary["THE_ARCHITECT.talk.DANJIN_CHARACTER_DANJIN.2-attack"] = "Both";
		}
	}
}
