using System;
using System.Collections.Generic;
using System.Linq;
using Danjin.Relics;
using Danjin.Variables;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;

namespace Danjin.Patches;

[HarmonyPatch(/*Could not decode attribute arguments.*/)]
internal static class FeiRenLiaoYuDynamicHoverTip
{
	private const string FeiRenTipId = "LocString with Title=card_keywords.DANJIN_KEYWORD_FEIREN.title and Description=card_keywords.DANJIN_KEYWORD_FEIREN.description";

	private const string LiaoYuTipId = "LocString with Title=card_keywords.DANJIN_KEYWORD_LIAOYU.title and Description=card_keywords.DANJIN_KEYWORD_LIAOYU.description";

	private static void Postfix(CardModel __instance, ref IEnumerable<IHoverTip> __result)
	{
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_021a: Unknown result type (might be due to invalid IL or missing references)
		//IL_021f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0331: Unknown result type (might be due to invalid IL or missing references)
		//IL_0350: Expected O, but got Unknown
		//IL_0397: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_017f: Expected O, but got Unknown
		//IL_01c5: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			if (!((AbstractModel)__instance).IsMutable || !CombatManager.Instance.IsInProgress)
			{
				return;
			}
			Player owner = __instance.Owner;
			if (((owner != null) ? owner.Creature : null) == null)
			{
				return;
			}
			Creature creature = __instance.Owner.Creature;
			List<IHoverTip> list = __result.ToList();
			bool flag = false;
			int num = list.FindIndex((IHoverTip t) => t.Id == "LocString with Title=card_keywords.DANJIN_KEYWORD_FEIREN.title and Description=card_keywords.DANJIN_KEYWORD_FEIREN.description");
			if (num >= 0 && list[num] is HoverTip val)
			{
				Player player = creature.Player;
				XuePing xuePing = ((player != null) ? player.GetRelic<XuePing>() : null);
				string text;
				if (xuePing != null && !xuePing.Consumed)
				{
					text = "\n[color=#808080]因血瓶效果，此卡牌不消耗生命值。[/color]";
				}
				else
				{
					decimal percent = 0.03m;
					FeiRenVar feiRenVar = __instance.DynamicVars.Values.OfType<FeiRenVar>().FirstOrDefault();
					if (feiRenVar != null)
					{
						percent = feiRenVar.PerHitPercent;
					}
					int hitCount = ((!(__instance is IFeirenHitCountProvider feirenHitCountProvider)) ? 1 : feirenHitCountProvider.GetFeirenHitCountForPreview(null));
					int value = FeiRenVar.CalculateEffectiveTotalHpLoss(creature, percent, hitCount);
					text = $"\n[color=#e06060]失去[b]{value}[/b]点生命值。[/color]";
				}
				HoverTip val2 = default(HoverTip);
				((HoverTip)(ref val2))._002Ector(new LocString("card_keywords", "DANJIN_KEYWORD_FEIREN.title"), ((HoverTip)(ref val)).Description + text, ((HoverTip)(ref val)).Icon);
				((HoverTip)(ref val2)).Id = "danjin-feiren-dynamic";
				((HoverTip)(ref val2)).IsSmart = ((HoverTip)(ref val)).IsSmart;
				((HoverTip)(ref val2)).IsDebuff = ((HoverTip)(ref val)).IsDebuff;
				((HoverTip)(ref val2)).IsInstanced = ((HoverTip)(ref val)).IsInstanced;
				((HoverTip)(ref val2)).ShouldOverrideTextOverflow = ((HoverTip)(ref val)).ShouldOverrideTextOverflow;
				list[num] = (IHoverTip)(object)val2;
				flag = true;
			}
			int num2 = list.FindIndex((IHoverTip t) => t.Id == "LocString with Title=card_keywords.DANJIN_KEYWORD_LIAOYU.title and Description=card_keywords.DANJIN_KEYWORD_LIAOYU.description");
			if (num2 >= 0 && list[num2] is HoverTip val3)
			{
				decimal percentPerStar = 0.03m;
				LiaoYuVar liaoYuVar = __instance.DynamicVars.Values.OfType<LiaoYuVar>().FirstOrDefault();
				if (liaoYuVar != null)
				{
					percentPerStar = liaoYuVar.PercentPerStar;
				}
				int starCost = Math.Max(0, __instance.GetStarCostWithModifiers());
				int num3 = LiaoYuVar.CalculateDesiredHeal(creature, starCost, percentPerStar);
				int num4 = LiaoYuVar.ApplyBloodPoolCap(__instance.Owner, num3);
				string text2 = ((num3 <= 0 || num4 >= num3) ? $"\n[color=#60c060]回复[b]{num4}[/b]点生命值。[/color]" : $"\n[color=#60c060]回复[b]{num4}[/b]点生命值[/color][color=#808080](理想{num3}，血池剩余仅可回 {num4})[/color]");
				HoverTip val4 = default(HoverTip);
				((HoverTip)(ref val4))._002Ector(new LocString("card_keywords", "DANJIN_KEYWORD_LIAOYU.title"), ((HoverTip)(ref val3)).Description + text2, ((HoverTip)(ref val3)).Icon);
				((HoverTip)(ref val4)).Id = "danjin-liaoyu-dynamic";
				((HoverTip)(ref val4)).IsSmart = ((HoverTip)(ref val3)).IsSmart;
				((HoverTip)(ref val4)).IsDebuff = ((HoverTip)(ref val3)).IsDebuff;
				((HoverTip)(ref val4)).IsInstanced = ((HoverTip)(ref val3)).IsInstanced;
				((HoverTip)(ref val4)).ShouldOverrideTextOverflow = ((HoverTip)(ref val3)).ShouldOverrideTextOverflow;
				list[num2] = (IHoverTip)(object)val4;
				flag = true;
			}
			if (flag)
			{
				__result = list;
			}
		}
		catch (Exception value2)
		{
			Log.Error($"[DanjinMod] FeiRenLiaoYuDynamicHoverTip.Postfix 异常: {value2}", 1);
		}
	}
}
