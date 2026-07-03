using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Danjin.Variables;

public class ZhuShiIiDamageVar : DamageVar
{
	public ZhuShiIiDamageVar(decimal baseValue)
		: base(baseValue, (ValueProp)8)
	{
	}

	public override void UpdateCardPreview(CardModel card, CardPreviewMode previewMode, Creature? target, bool runGlobalHooks)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
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
		if (target != null && card.DynamicVars.TryGetValue("ZhuShiDamageBonus", ref val))
		{
			ZhuShiZhiKePower power = target.GetPower<ZhuShiZhiKePower>();
			int num = ((power != null) ? ((PowerModel)power).Amount : 0);
			baseValue2 += (decimal)num * val.BaseValue;
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
