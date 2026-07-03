using System.Collections.Generic;
using Danjin.Powers;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Danjin.Variables;

public class QingSuanDamageVar(decimal baseValue) : DamageVar(baseValue, (ValueProp)8)
{
	private readonly decimal _rawBase = baseValue;

	public override void UpdateCardPreview(CardModel card, CardPreviewMode previewMode, Creature? target, bool runGlobalHooks)
	{
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		Player owner = card.Owner;
		decimal num = SumBlockedDamage((owner != null) ? owner.Creature : null);
		((DynamicVar)this).BaseValue = baseValue + num;
		decimal baseValue = ((DynamicVar)this).BaseValue;
		Player owner2 = card.Owner;
		int? obj;
		if (owner2 == null)
		{
			obj = null;
		}
		else
		{
			Creature creature = owner2.Creature;
			obj = ((creature != null) ? new int?(creature.GetPowerAmount<ShouShiPower>()) : ((int?)null));
		}
		int? num2 = obj;
		if (num2.GetValueOrDefault() >= 4)
		{
			baseValue *= 2m;
		}
		EnchantmentModel enchantment = card.Enchantment;
		if (enchantment != null)
		{
			decimal baseValue2 = ((DynamicVar)this).BaseValue;
			baseValue2 += enchantment.EnchantDamageAdditive(baseValue2, ((DamageVar)this).Props);
			baseValue2 *= enchantment.EnchantDamageMultiplicative(baseValue2, ((DamageVar)this).Props);
			if (!card.IsEnchantmentPreview)
			{
				((DynamicVar)this).EnchantedValue = baseValue2;
			}
		}
		if (runGlobalHooks)
		{
			IEnumerable<AbstractModel> enumerable = default(IEnumerable<AbstractModel>);
			((DynamicVar)this).PreviewValue = Hook.ModifyDamage(card.Owner.RunState, card.CombatState, target, card.Owner.Creature, baseValue, ((DamageVar)this).Props, card, (ModifyDamageHookType)14, previewMode, ref enumerable);
			return;
		}
		decimal num3 = baseValue;
		if (enchantment != null)
		{
			num3 += enchantment.EnchantDamageAdditive(num3, ((DamageVar)this).Props);
			num3 *= enchantment.EnchantDamageMultiplicative(num3, ((DamageVar)this).Props);
		}
		((DynamicVar)this).PreviewValue = num3;
	}

	public static int SumBlockedDamage(Creature? receiver)
	{
		if (receiver == null)
		{
			return 0;
		}
		CombatManager instance = CombatManager.Instance;
		CombatHistory val = ((instance != null) ? instance.History : null);
		if (val == null)
		{
			return 0;
		}
		int num = 0;
		foreach (CombatHistoryEntry entry in val.Entries)
		{
			CreatureAttackedEntry val2 = (CreatureAttackedEntry)(object)((entry is CreatureAttackedEntry) ? entry : null);
			if (val2 == null)
			{
				continue;
			}
			foreach (DamageResult damageResult in val2.DamageResults)
			{
				if (damageResult.Receiver == receiver)
				{
					num += damageResult.BlockedDamage;
				}
			}
		}
		return num;
	}
}
