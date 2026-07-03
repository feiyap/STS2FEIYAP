using System;
using System.Linq;
using System.Threading.Tasks;
using Danjin.Extensions;
using Danjin.Hooks;
using Danjin.Powers;
using Danjin.Relics;
using Danjin.Utils;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

public sealed class ZhuShiZhiKePower : DanjinPower, IDanjinHealAmountModifier
{
	public const decimal BaseDamageBonus = 0.25m;

	public const decimal BaseBlockReduction = 0.50m;

	public const decimal BaseHealReduction = 0.50m;

	public const decimal EnhancedDamageBonus = 0.75m;

	public const decimal EnhancedBlockReduction = 0.50m;

	public const decimal EnhancedHealReduction = 0.50m;

	public const decimal JiChouBonus = 0.25m;

	private int _lastReducedRound = -1;

	public override PowerType Type => (PowerType)2;

	public override PowerStackType StackType => (PowerStackType)1;

	public override string CustomIconPath
	{
		get
		{
			if (IsZhuShiInvalidated())
			{
				string text = "zhushizhike_gray.png".PowerImagePath();
				if (ResourceLoader.Exists(text, ""))
				{
					return text;
				}
			}
			return base.CustomIconPath;
		}
	}

	private static bool IsPoweredAttack(ValueProp props)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		if (((Enum)props).HasFlag((Enum)(object)(ValueProp)8))
		{
			return !((Enum)props).HasFlag((Enum)(object)(ValueProp)4);
		}
		return false;
	}

	private bool AnyPlayerHasTeZhiLiaoHuan()
	{
		try
		{
			Creature owner = ((PowerModel)this).Owner;
			bool? obj;
			if (owner == null)
			{
				obj = null;
			}
			else
			{
				ICombatState combatState = owner.CombatState;
				obj = ((combatState != null) ? new bool?(combatState.Players.Any((Player p) => p.Relics.Any((RelicModel r) => r is TeZhiLiaoHuan))) : ((bool?)null));
			}
			bool? flag = obj;
			return flag == true;
		}
		catch
		{
			return false;
		}
	}

	private bool OwnerHasJiChou()
	{
		Creature owner = ((PowerModel)this).Owner;
		if (owner == null)
		{
			return false;
		}
		return owner.HasPower<JiChouPower>();
	}

	private bool IsZhuShiInvalidated()
	{
		try
		{
			Creature owner = ((PowerModel)this).Owner;
			bool? obj;
			if (owner == null)
			{
				obj = null;
			}
			else
			{
				ICombatState combatState = owner.CombatState;
				obj = ((combatState != null) ? new bool?(combatState.Players.Any(delegate(Player p)
				{
					Creature creature = p.Creature;
					return creature != null && creature.HasPower<ZhuShiInvalidatedPower>();
				})) : ((bool?)null));
			}
			bool? flag = obj;
			return flag == true;
		}
		catch
		{
			return false;
		}
	}

	private bool AnyPlayerHasChenShengYanMie()
	{
		try
		{
			Creature owner = ((PowerModel)this).Owner;
			bool? obj;
			if (owner == null)
			{
				obj = null;
			}
			else
			{
				ICombatState combatState = owner.CombatState;
				obj = ((combatState != null) ? new bool?(combatState.Players.Any(delegate(Player p)
				{
					Creature creature = p.Creature;
					return creature != null && creature.HasPower<ChenShengYanMiePower>();
				})) : ((bool?)null));
			}
			bool? flag = obj;
			return flag == true;
		}
		catch
		{
			return false;
		}
	}

	public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		if (target != ((PowerModel)this).Owner)
		{
			return 1m;
		}
		if (!IsPoweredAttack(props))
		{
			return 1m;
		}
		if (IsZhuShiInvalidated())
		{
			return 1m;
		}
		decimal num = (AnyPlayerHasTeZhiLiaoHuan() ? 0.75m : 0.25m);
		if (OwnerHasJiChou())
		{
			num += 0.25m;
		}
		decimal num2 = 1m + num;
		if (amount > 0m)
		{
			decimal num3 = Math.Ceiling(amount * num2);
			DanjinLog.Verbose($">>>[DanjinMod] 朱蚀之刻({((PowerModel)this).Amount}层): 伤害 ×{num2}({amount} → {num3}, 向上取整)");
			return num3 / amount;
		}
		DanjinLog.Verbose($">>>[DanjinMod] 朱蚀之刻({((PowerModel)this).Amount}层): 伤害 ×{num2}({amount} → {amount * num2})");
		return num2;
	}

	public override decimal ModifyBlockMultiplicative(Creature target, decimal block, ValueProp props, CardModel? cardSource, CardPlay? cardPlay)
	{
		if (target != ((PowerModel)this).Owner)
		{
			return 1m;
		}
		if (IsZhuShiInvalidated())
		{
			return 1m;
		}
		decimal num = (AnyPlayerHasTeZhiLiaoHuan() ? 0.50m : 0.50m);
		if (OwnerHasJiChou())
		{
			num += 0.25m;
		}
		if (AnyPlayerHasChenShengYanMie())
		{
			num = 1m;
		}
		decimal num2 = 1m - num;
		if (num2 < 0m)
		{
			num2 = default(decimal);
		}
		return num2;
	}

	public decimal ModifyHealMultiplicative(Creature creature, decimal amount)
	{
		if (creature != ((PowerModel)this).Owner)
		{
			return 1m;
		}
		if (IsZhuShiInvalidated())
		{
			return 1m;
		}
		decimal num = (AnyPlayerHasTeZhiLiaoHuan() ? 0.50m : 0.50m);
		if (OwnerHasJiChou())
		{
			num += 0.25m;
		}
		if (AnyPlayerHasChenShengYanMie())
		{
			num = 1m;
		}
		decimal num2 = 1m - num;
		if (num2 < 0m)
		{
			num2 = default(decimal);
		}
		return num2;
	}

	public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
	{
		ICombatState combatState = ((PowerModel)this).Owner.CombatState;
		int currentRound = ((combatState != null) ? combatState.RoundNumber : 0);
		if (currentRound != _lastReducedRound)
		{
			_lastReducedRound = currentRound;
			await PowerCmd.Apply<ZhuShiZhiKePower>(choiceContext, ((PowerModel)this).Owner, -1m, ((PowerModel)this).Owner, (CardModel)null, true);
			DanjinLog.Verbose($">>>[DanjinMod] 朱蚀之刻：回合减层(轮次{currentRound})→ 剩余 {((PowerModel)this).Amount - 1} 层");
		}
	}
}
