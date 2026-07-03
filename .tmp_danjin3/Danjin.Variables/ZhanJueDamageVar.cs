using System.Collections.Generic;
using Danjin.Powers;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Danjin.Variables;

public class ZhanJueDamageVar : DamageVar
{
	public ZhanJueDamageVar(decimal baseValue)
		: base(baseValue, (ValueProp)8)
	{
	}

	public override void UpdateCardPreview(CardModel card, CardPreviewMode previewMode, Creature? target, bool runGlobalHooks)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		decimal baseValue = ((DynamicVar)this).BaseValue;
		EnchantmentModel enchantment = card.Enchantment;
		if (enchantment != null)
		{
			baseValue += enchantment.EnchantDamageAdditive(baseValue, ((DamageVar)this).Props);
			baseValue *= enchantment.EnchantDamageMultiplicative(baseValue, ((DamageVar)this).Props);
			if (!card.IsEnchantmentPreview)
			{
				((DynamicVar)this).EnchantedValue = baseValue;
			}
		}
		decimal baseValue2 = ((DynamicVar)this).BaseValue;
		DynamicVar val = default(DynamicVar);
		if (card.DynamicVars.TryGetValue("BonusPerSwitch", ref val))
		{
			Player owner = card.Owner;
			int? obj;
			if (owner == null)
			{
				obj = null;
			}
			else
			{
				Creature creature = owner.Creature;
				obj = ((creature != null) ? new int?(creature.GetPowerAmount<StanceSwitchCountPower>()) : ((int?)null));
			}
			int? num = obj;
			int valueOrDefault = num.GetValueOrDefault();
			if (valueOrDefault > 0)
			{
				baseValue2 += (decimal)valueOrDefault * val.BaseValue;
			}
		}
		if (runGlobalHooks)
		{
			IEnumerable<AbstractModel> enumerable = default(IEnumerable<AbstractModel>);
			baseValue = Hook.ModifyDamage(card.Owner.RunState, card.CombatState, target, card.Owner.Creature, baseValue2, ((DamageVar)this).Props, card, (ModifyDamageHookType)14, previewMode, ref enumerable);
		}
		else
		{
			baseValue = baseValue2;
			if (enchantment != null)
			{
				baseValue += enchantment.EnchantDamageAdditive(baseValue, ((DamageVar)this).Props);
				baseValue *= enchantment.EnchantDamageMultiplicative(baseValue, ((DamageVar)this).Props);
			}
		}
		((DynamicVar)this).PreviewValue = baseValue;
	}
}
