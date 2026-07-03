using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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

[HarmonyPatch(typeof(Hook), "AfterCardPlayed")]
internal static class LianDuanPatch
{
	internal static readonly HashSet<CardModel> RecentlyGenerated = new HashSet<CardModel>();

	[HarmonyPostfix]
	private static void Postfix(CardPlay cardPlay, ref Task __result)
	{
		Task original = __result ?? Task.CompletedTask;
		__result = ExhaustLianDuanAfter(original, cardPlay);
	}

	private static async Task ExhaustLianDuanAfter(Task original, CardPlay cardPlay)
	{
		await original;
		HashSet<CardModel> grace = ((RecentlyGenerated.Count > 0) ? new HashSet<CardModel>(RecentlyGenerated) : null);
		RecentlyGenerated.Clear();
		try
		{
			if (CombatManager.Instance == null || !CombatManager.Instance.IsInProgress)
			{
				return;
			}
			CardModel val = ((cardPlay != null) ? cardPlay.Card : null);
			Player val2 = ((val != null) ? val.Owner : null);
			if (val2 == null)
			{
				return;
			}
			if (val != null && val.Keywords.Contains(DanjinCardKeywords.LianDuan))
			{
				await CardPileCmd.Add(val, (PileType)4, (CardPilePosition)1, (AbstractModel)null, false);
				return;
			}
			CardPile pile = PileTypeExtensions.GetPile((PileType)2, val2);
			if (pile == null)
			{
				return;
			}
			List<CardModel> list = pile.Cards.Where((CardModel c) => c != null && c.Keywords.Contains(DanjinCardKeywords.LianDuan) && (grace == null || !grace.Contains(c))).ToList();
			if (list.Count == 0)
			{
				return;
			}
			foreach (CardModel item in list)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(38, 2);
				defaultInterpolatedStringHandler.AppendLiteral(">>>[DanjinMod] 连段：打出 [");
				object value;
				if (cardPlay == null)
				{
					value = null;
				}
				else
				{
					CardModel card = cardPlay.Card;
					value = ((card != null) ? card.Title : null);
				}
				defaultInterpolatedStringHandler.AppendFormatted((string?)value);
				defaultInterpolatedStringHandler.AppendLiteral("] 后，消耗手牌中的连段卡 [");
				defaultInterpolatedStringHandler.AppendFormatted(item.Title);
				defaultInterpolatedStringHandler.AppendLiteral("]");
				DanjinLog.Verbose(defaultInterpolatedStringHandler.ToStringAndClear());
				await CardPileCmd.Add(item, (PileType)4, (CardPilePosition)1, (AbstractModel)null, false);
			}
		}
		catch (Exception value2)
		{
			Log.Error($"[Danjin] LianDuanPatch 消耗连段卡异常: {value2}", 2);
		}
	}
}
