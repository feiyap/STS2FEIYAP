using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Danjin.Cards;
using Danjin.Utils;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;

namespace Danjin.Patches;

[HarmonyPatch(typeof(Hook), "BeforeTurnEnd")]
internal static class LianDuanEtherealPatch
{
	[HarmonyPostfix]
	private static void Postfix(ICombatState combatState, CombatSide side, ref Task __result)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		Task original = __result ?? Task.CompletedTask;
		__result = ExhaustEtherealLianDuan(original, combatState, side);
	}

	private static async Task ExhaustEtherealLianDuan(Task original, ICombatState combatState, CombatSide side)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		await original;
		try
		{
			if ((int)side != 1 || combatState == null || CombatManager.Instance == null || !CombatManager.Instance.IsInProgress)
			{
				return;
			}
			foreach (Player player in combatState.Players)
			{
				if (player == null)
				{
					continue;
				}
				CardPile pile = PileTypeExtensions.GetPile((PileType)2, player);
				if (pile == null)
				{
					continue;
				}
				List<CardModel> list = pile.Cards.Where((CardModel c) => c != null && c.Keywords.Contains(DanjinCardKeywords.LianDuan)).ToList();
				foreach (CardModel item in list)
				{
					DanjinLog.Verbose(">>>[DanjinMod] 连段[虚无]：玩家回合结束，消耗手牌中的连段卡 [" + item.Title + "]");
					await CardPileCmd.Add(item, (PileType)4, (CardPilePosition)1, (AbstractModel)null, false);
				}
			}
		}
		catch (Exception value)
		{
			Log.Error($"[Danjin] LianDuanEtherealPatch 异常: {value}", 2);
		}
	}
}
