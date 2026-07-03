using System;
using System.Threading.Tasks;
using Danjin.Cards;
using Danjin.Character;
using Danjin.Powers;
using Danjin.Resources;
using Danjin.Utils;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;

namespace Danjin.Patches;

internal static class DanjinStancePatch
{
	[HarmonyPatch(typeof(Hook), "AfterCardPlayed")]
	private static class AttackStancePatch
	{
		[HarmonyPostfix]
		private static void Postfix(PlayerChoiceContext choiceContext, CardPlay cardPlay, ref Task __result)
		{
			Task original = __result ?? Task.CompletedTask;
			__result = GainAfter(original, choiceContext, cardPlay);
		}

		private static async Task GainAfter(Task original, PlayerChoiceContext ctx, CardPlay cardPlay)
		{
			await original;
			try
			{
				if (CombatManager.Instance == null || !CombatManager.Instance.IsInProgress)
				{
					return;
				}
				CardModel val = ((cardPlay != null) ? cardPlay.Card : null);
				if (val == null)
				{
					return;
				}
				if (!(val.Pool is DanjinCardPool))
				{
					Player owner = val.Owner;
					if (!(((owner != null) ? owner.Character : null) is Danjin.Character.Danjin))
					{
						return;
					}
				}
				if ((int)val.Type != 1 || HasStanceControlKeyword(val))
				{
					return;
				}
				Player owner2 = val.Owner;
				if (owner2 != null)
				{
					DanjinLog.Verbose(">>>[DanjinMod] 基础被动：打出攻击牌 [" + val.Title + "] → +1 攻势");
					await StanceCmd.GainAttackStance(ctx, owner2);
					Creature creature = owner2.Creature;
					XiuLuoXingTaiPower xiuLuoXingTaiPower = ((creature != null) ? creature.GetPower<XiuLuoXingTaiPower>() : null);
					if (xiuLuoXingTaiPower != null)
					{
						await xiuLuoXingTaiPower.Resync(ctx);
					}
				}
			}
			catch (Exception value)
			{
				Log.Error($"[Danjin] DanjinStancePatch.AttackStance 异常: {value}", 2);
			}
		}
	}

	[HarmonyPatch(typeof(Hook), "AfterBlockGained")]
	private static class DefenseStancePatch
	{
		[HarmonyPostfix]
		private static void Postfix(Creature creature, decimal amount, CardModel cardSource, ref Task __result)
		{
			Task original = __result ?? Task.CompletedTask;
			__result = GainAfter(original, creature, amount, cardSource);
		}

		private static async Task GainAfter(Task original, Creature creature, decimal amount, CardModel? cardSource)
		{
			await original;
			try
			{
				if (CombatManager.Instance == null || !CombatManager.Instance.IsInProgress || creature == null || amount <= 0m || cardSource == null)
				{
					return;
				}
				if (!(cardSource.Pool is DanjinCardPool))
				{
					object obj;
					if (creature == null)
					{
						obj = null;
					}
					else
					{
						Player player = creature.Player;
						obj = ((player != null) ? player.Character : null);
					}
					if (!(obj is Danjin.Character.Danjin))
					{
						return;
					}
				}
				if ((int)cardSource.Type != 2 || HasStanceControlKeyword(cardSource))
				{
					return;
				}
				Player player2 = creature.Player;
				if (player2 != null)
				{
					DanjinLog.Verbose(">>>[DanjinMod] 基础被动：通过技能牌 [" + cardSource.Title + "] 获得格挡 → +1 守势");
					await StanceCmd.GainDefenseStance((PlayerChoiceContext)new BlockingPlayerChoiceContext(), player2);
					Creature creature2 = player2.Creature;
					XiuLuoXingTaiPower xiuLuoXingTaiPower = ((creature2 != null) ? creature2.GetPower<XiuLuoXingTaiPower>() : null);
					if (xiuLuoXingTaiPower != null)
					{
						await xiuLuoXingTaiPower.Resync((PlayerChoiceContext)new BlockingPlayerChoiceContext());
					}
				}
			}
			catch (Exception value)
			{
				Log.Error($"[Danjin] DanjinStancePatch.DefenseStance 异常: {value}", 2);
			}
		}
	}

	private static bool HasStanceControlKeyword(CardModel card)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		if (!card.Keywords.Contains(DanjinCardKeywords.LiuZhuan))
		{
			return card.Keywords.Contains(DanjinCardKeywords.CanXin);
		}
		return true;
	}
}
