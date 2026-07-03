using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Danjin.Powers;
using Danjin.Relics;
using Danjin.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Danjin.Resources;

public static class StanceCmd
{
	private enum StanceDamageMode
	{
		None,
		RandomSingle,
		Aoe
	}

	private static bool _resolving;

	public const int DefaultMaxStacks = 4;

	public const int FeiYuMaxStacks = 6;

	public static int GetMaxStacks(Player player)
	{
		int num = 4;
		if (((player != null) ? player.GetRelic<FeiYu>() : null) != null)
		{
			num += 2;
		}
		Creature obj = ((player != null) ? player.Creature : null);
		XiuLuoXingTaiPower xiuLuoXingTaiPower = ((obj != null) ? obj.GetPower<XiuLuoXingTaiPower>() : null);
		if (xiuLuoXingTaiPower != null)
		{
			num += ((PowerModel)xiuLuoXingTaiPower).Amount;
		}
		ManHuangZhiDiDeFeiSePower manHuangZhiDiDeFeiSePower = ((obj != null) ? obj.GetPower<ManHuangZhiDiDeFeiSePower>() : null);
		if (manHuangZhiDiDeFeiSePower != null)
		{
			num -= ((PowerModel)manHuangZhiDiDeFeiSePower).Amount;
		}
		return Math.Max(0, num);
	}

	public static async Task ClearStanceFreeze(PlayerChoiceContext choiceContext, Player player)
	{
		Creature val = ((player != null) ? player.Creature : null);
		if (val == null)
		{
			return;
		}
		List<PowerModel> list = val.Powers.Where((PowerModel p) => p is IStanceFreezePower).ToList();
		foreach (PowerModel item in list)
		{
			await PowerCmd.Remove(item);
		}
	}

	public static async Task GainAttackStance(PlayerChoiceContext choiceContext, Player player, int amount = 1)
	{
		if (player == null || amount <= 0 || _resolving)
		{
			return;
		}
		if (player.GetRelic<QianGuFuLiu>() != null)
		{
			amount *= 2;
		}
		Creature creature = player.Creature;
		if (creature == null || !creature.IsAlive || CombatManager.Instance == null || !CombatManager.Instance.IsInProgress)
		{
			return;
		}
		if (IsStanceTypeBlocked(creature, toAttack: true))
		{
			DanjinLog.Verbose(">>>[DanjinMod] 架势种类锁定，跳过获得攻势");
			return;
		}
		int max = GetMaxStacks(player);
		if (max <= 0)
		{
			await ClearAllStances(choiceContext, creature);
			return;
		}
		int curAttack = creature.GetPowerAmount<GongShiPower>();
		if (curAttack >= max)
		{
			return;
		}
		_resolving = true;
		try
		{
			await ClearDefenseStanceAndConvert(choiceContext, player, creature);
			int target = Math.Min(curAttack + amount, max);
			await SetPowerTo<GongShiPower>(choiceContext, creature, target);
		}
		finally
		{
			_resolving = false;
		}
	}

	public static async Task GainDefenseStance(PlayerChoiceContext choiceContext, Player player, int amount = 1)
	{
		if (player == null || amount <= 0 || _resolving)
		{
			return;
		}
		if (player.GetRelic<QianGuFuLiu>() != null)
		{
			amount *= 2;
		}
		Creature creature = player.Creature;
		if (creature == null || !creature.IsAlive || CombatManager.Instance == null || !CombatManager.Instance.IsInProgress)
		{
			return;
		}
		if (IsStanceTypeBlocked(creature, toAttack: false))
		{
			DanjinLog.Verbose(">>>[DanjinMod] 架势种类锁定，跳过获得守势");
			return;
		}
		int max = GetMaxStacks(player);
		if (max <= 0)
		{
			await ClearAllStances(choiceContext, creature);
			return;
		}
		int curDefense = creature.GetPowerAmount<ShouShiPower>();
		if (curDefense >= max)
		{
			return;
		}
		_resolving = true;
		try
		{
			await ClearAttackStanceAndConvert(choiceContext, player, creature);
			int target = Math.Min(curDefense + amount, max);
			await SetPowerTo<ShouShiPower>(choiceContext, creature, target);
		}
		finally
		{
			_resolving = false;
		}
	}

	public static async Task SwitchStance(PlayerChoiceContext choiceContext, Player player)
	{
		if (player == null || _resolving)
		{
			return;
		}
		Creature creature = player.Creature;
		if (creature == null || !creature.IsAlive || CombatManager.Instance == null || !CombatManager.Instance.IsInProgress)
		{
			return;
		}
		if (creature.Powers.Any((PowerModel p) => p is IStanceFreezePower))
		{
			DanjinLog.Verbose(">>>[DanjinMod] 架势种类锁定，跳过流转");
			return;
		}
		int a = creature.GetPowerAmount<GongShiPower>();
		int d = creature.GetPowerAmount<ShouShiPower>();
		int max = GetMaxStacks(player);
		if (max > 0)
		{
			_resolving = true;
			try
			{
				if (a > 0)
				{
					DanjinLog.Verbose($">>>[DanjinMod] 流转：攻势 {a} → 守势 {Math.Min(a, max)}");
					await ClearAttackStanceAndConvert(choiceContext, player, creature);
					await SetPowerTo<ShouShiPower>(choiceContext, creature, Math.Min(a, max));
				}
				else if (d > 0)
				{
					DanjinLog.Verbose($">>>[DanjinMod] 流转：守势 {d} → 攻势 {Math.Min(d, max)}");
					await ClearDefenseStanceAndConvert(choiceContext, player, creature);
					await SetPowerTo<GongShiPower>(choiceContext, creature, Math.Min(d, max));
				}
				else
				{
					DanjinLog.Verbose(">>>[DanjinMod] 流转：当前无架势 → 默认进入守势 1");
					await SetPowerTo<ShouShiPower>(choiceContext, creature, 1);
				}
				return;
			}
			finally
			{
				_resolving = false;
			}
		}
		await ClearAllStances(choiceContext, creature);
	}

	public static async Task SwitchToStance(PlayerChoiceContext choiceContext, Player player, bool toAttack)
	{
		if (player == null || _resolving)
		{
			return;
		}
		Creature creature = player.Creature;
		if (creature == null || !creature.IsAlive || CombatManager.Instance == null || !CombatManager.Instance.IsInProgress)
		{
			return;
		}
		if (IsStanceTypeBlocked(creature, toAttack))
		{
			DanjinLog.Verbose(">>>[DanjinMod] 架势种类锁定，跳过定向切换");
			return;
		}
		int a = creature.GetPowerAmount<GongShiPower>();
		int d = creature.GetPowerAmount<ShouShiPower>();
		int max = GetMaxStacks(player);
		if (max > 0)
		{
			_resolving = true;
			try
			{
				if (toAttack)
				{
					if (a <= 0 && d > 0)
					{
						DanjinLog.Verbose($">>>[DanjinMod] 定向切换：守势 {d} → 攻势 {Math.Min(d, max)}");
						await ClearDefenseStanceAndConvert(choiceContext, player, creature);
						await SetPowerTo<GongShiPower>(choiceContext, creature, Math.Min(d, max));
					}
				}
				else if (d <= 0 && a > 0)
				{
					DanjinLog.Verbose($">>>[DanjinMod] 定向切换：攻势 {a} → 守势 {Math.Min(a, max)}");
					await ClearAttackStanceAndConvert(choiceContext, player, creature);
					await SetPowerTo<ShouShiPower>(choiceContext, creature, Math.Min(a, max));
				}
				return;
			}
			finally
			{
				_resolving = false;
			}
		}
		await ClearAllStances(choiceContext, creature);
	}

	private static async Task ClearAllStances(PlayerChoiceContext choiceContext, Creature creature)
	{
		DanjinLog.Verbose(">>>[DanjinMod] 架势上限 ≤ 0，移除所有架势");
		if (creature.GetPowerAmount<GongShiPower>() > 0)
		{
			await PowerCmd.Remove<GongShiPower>(creature);
		}
		if (creature.GetPowerAmount<ShouShiPower>() > 0)
		{
			await PowerCmd.Remove<ShouShiPower>(creature);
		}
	}

	private static bool IsStanceTypeBlocked(Creature creature, bool toAttack)
	{
		int num = (toAttack ? creature.GetPowerAmount<ShouShiPower>() : creature.GetPowerAmount<GongShiPower>());
		foreach (PowerModel power in creature.Powers)
		{
			if (!(power is IStanceFreezePower { LockedToAttack: var lockedToAttack }))
			{
				continue;
			}
			if (lockedToAttack.HasValue)
			{
				if (lockedToAttack.Value != toAttack)
				{
					return true;
				}
			}
			else if (num > 0)
			{
				return true;
			}
		}
		return false;
	}

	public static async Task ReinforceStance(PlayerChoiceContext choiceContext, Player player)
	{
		if (player == null || _resolving)
		{
			return;
		}
		Creature creature = player.Creature;
		if (creature == null || !creature.IsAlive || CombatManager.Instance == null || !CombatManager.Instance.IsInProgress)
		{
			return;
		}
		int powerAmount = creature.GetPowerAmount<GongShiPower>();
		int powerAmount2 = creature.GetPowerAmount<ShouShiPower>();
		int maxStacks = GetMaxStacks(player);
		if (maxStacks > 0)
		{
			_resolving = true;
			try
			{
				if (powerAmount > 0)
				{
					await SetPowerTo<GongShiPower>(choiceContext, creature, Math.Min(powerAmount + 1, maxStacks));
				}
				else if (powerAmount2 > 0)
				{
					await SetPowerTo<ShouShiPower>(choiceContext, creature, Math.Min(powerAmount2 + 1, maxStacks));
				}
				else
				{
					await SetPowerTo<GongShiPower>(choiceContext, creature, 1);
				}
				return;
			}
			finally
			{
				_resolving = false;
			}
		}
		await ClearAllStances(choiceContext, creature);
	}

	private static async Task OnStanceSwitched(PlayerChoiceContext choiceContext, Player player, Creature creature)
	{
		await PowerCmd.Apply<StanceSwitchCountPower>(choiceContext, creature, 1m, creature, (CardModel)null, true);
		GangRouHuaShiPower power = creature.GetPower<GangRouHuaShiPower>();
		if (power != null && ((PowerModel)power).Amount > 0)
		{
			await CardPileCmd.Draw(choiceContext, (decimal)((PowerModel)power).Amount, player, false);
		}
	}

	private static async Task ClearAttackStanceAndConvert(PlayerChoiceContext choiceContext, Player player, Creature creature)
	{
		int a = creature.GetPowerAmount<GongShiPower>();
		if (a > 0)
		{
			await OnStanceSwitched(choiceContext, player, creature);
			int block = GetAttackClearBlock(player, a);
			await RemovePower<GongShiPower>(creature);
			if (block > 0)
			{
				DanjinLog.Verbose($">>>[DanjinMod] 攻守：清空攻势 {a} 层 → 获得 {block} 点格挡");
				await CreatureCmd.GainBlock(creature, (decimal)block, (ValueProp)4, (CardPlay)null, false);
			}
		}
	}

	private static async Task ClearDefenseStanceAndConvert(PlayerChoiceContext choiceContext, Player player, Creature creature)
	{
		int d = creature.GetPowerAmount<ShouShiPower>();
		if (d <= 0)
		{
			return;
		}
		await OnStanceSwitched(choiceContext, player, creature);
		StanceDamageMode mode = GetDefenseClearDamageMode(player);
		await RemovePower<ShouShiPower>(creature);
		if (mode == StanceDamageMode.None)
		{
			return;
		}
		ICombatState combatState = creature.CombatState;
		List<Creature> list = ((combatState != null) ? combatState.HittableEnemies.Where((Creature e) => e.IsAlive).ToList() : null);
		if (list != null && list.Count != 0)
		{
			if (mode == StanceDamageMode.Aoe)
			{
				DanjinLog.Verbose($">>>[DanjinMod] 攻守：清空守势 {d} 层 → 对全体造成 {d} 点伤害");
				await CreatureCmd.Damage(choiceContext, (IEnumerable<Creature>)list, (decimal)d, (ValueProp)4, creature);
			}
			else
			{
				Creature val = player.RunState.Rng.CombatTargets.NextItem<Creature>((IEnumerable<Creature>)list);
				DanjinLog.Verbose($">>>[DanjinMod] 攻守：清空守势 {d} 层 → 随机对 [{((val != null) ? val.Name : null)}] 造成 {d} 点伤害");
				await CreatureCmd.Damage(choiceContext, (IEnumerable<Creature>)new List<Creature> { val }, (decimal)d, (ValueProp)4, creature);
			}
		}
	}

	private static int GetAttackClearBlock(Player player, int a)
	{
		if (a <= 0)
		{
			return 0;
		}
		if (((player != null) ? player.GetRelic<FeiYu>() : null) != null)
		{
			return a + 1;
		}
		if (((player != null) ? player.GetRelic<HuaiJinZhiYu>() : null) != null)
		{
			return a;
		}
		return 0;
	}

	private static StanceDamageMode GetDefenseClearDamageMode(Player player)
	{
		if (((player != null) ? player.GetRelic<FeiYu>() : null) != null)
		{
			return StanceDamageMode.Aoe;
		}
		if (((player != null) ? player.GetRelic<HuaiJinZhiYu>() : null) != null)
		{
			return StanceDamageMode.RandomSingle;
		}
		return StanceDamageMode.None;
	}

	private static async Task SetPowerTo<T>(PlayerChoiceContext choiceContext, Creature creature, int target) where T : PowerModel
	{
		if (target > 0 && (typeof(T) == typeof(GongShiPower) || typeof(T) == typeof(ShouShiPower)) && (creature.HasPower<ZhuJiTongXiaoPower>() || creature.HasPower<XiuLuoXingTaiPower>()) && creature.Player != null)
		{
			target = GetMaxStacks(creature.Player);
		}
		int powerAmount = creature.GetPowerAmount<T>();
		if (target <= 0)
		{
			await RemovePower<T>(creature);
			return;
		}
		int num = target - powerAmount;
		if (num == 0)
		{
			return;
		}
		if (num > 0)
		{
			await PowerCmd.Apply<T>(choiceContext, creature, (decimal)num, creature, (CardModel)null, true);
			return;
		}
		T power = creature.GetPower<T>();
		if (power != null)
		{
			((PowerModel)power).SetAmount(target, true);
		}
	}

	private static async Task RemovePower<T>(Creature creature) where T : PowerModel
	{
		T power = creature.GetPower<T>();
		if (power != null)
		{
			await PowerCmd.Remove((PowerModel)(object)power);
		}
	}
}
