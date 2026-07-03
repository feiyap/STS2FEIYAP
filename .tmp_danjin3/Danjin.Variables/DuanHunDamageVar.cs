using System.Collections.Generic;
using System.Linq;
using Danjin.Cards;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Danjin.Variables;

public class DuanHunDamageVar : DamageVar
{
	public DuanHunDamageVar(decimal baseValue)
		: base(baseValue, (ValueProp)8)
	{
	}

	public override void UpdateCardPreview(CardModel card, CardPreviewMode previewMode, Creature? target, bool runGlobalHooks)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
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
		if (card.CombatState != null && card.DynamicVars.TryGetValue("BonusDmgPerFeiRen", ref val))
		{
			int num = CombatManager.Instance.History.CardPlaysFinished.Count((CardPlayFinishedEntry e) => e.CardPlay.Card.Owner == card.Owner && e.CardPlay.Card.Keywords.Contains(DanjinCardKeywords.FeiRen));
			if (num > 0)
			{
				baseValue2 += (decimal)num * val.BaseValue;
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
