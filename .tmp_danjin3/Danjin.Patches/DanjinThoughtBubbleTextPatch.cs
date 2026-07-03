using System;
using Danjin.Character;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Vfx;

namespace Danjin.Patches;

[HarmonyPatch(typeof(NThoughtBubbleVfx), "Create", new Type[]
{
	typeof(string),
	typeof(Creature),
	typeof(double?)
})]
internal static class DanjinThoughtBubbleTextPatch
{
	[HarmonyPrefix]
	private static void Prefix(ref string text, Creature speaker)
	{
		try
		{
			if (speaker != null && speaker.IsPlayer && !string.IsNullOrEmpty(text) && text.Contains("辉星"))
			{
				CardModel lastFailedCard = CardPlayAttemptContext.LastFailedCard;
				if (lastFailedCard != null && lastFailedCard.Pool is DanjinCardPool)
				{
					text = text.Replace("辉星", "彤华");
					Log.Info(">>>[DanjinMod] 台词替换:辉星→彤华(失败卡='" + lastFailedCard.Title + "'),最终文本: " + text, 2);
				}
			}
		}
		catch (Exception ex)
		{
			Log.Info(">>>[DanjinMod] ThoughtBubble Prefix 异常: " + ex.Message, 2);
		}
	}
}
