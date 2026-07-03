using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Danjin.Cards;
using Danjin.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Danjin.Powers;

public sealed class JianSaoChunQiuPower : DanjinPower
{
	public override PowerType Type => (PowerType)1;

	public override PowerStackType StackType => (PowerStackType)2;

	public override async Task AfterDamageGiven(PlayerChoiceContext choiceContext, Creature? dealer, DamageResult result, ValueProp props, Creature target, CardModel? cardSource)
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		if (dealer != ((PowerModel)this).Owner || !ValuePropExtensions.IsPoweredAttack(props) || result.TotalDamage <= 0 || cardSource == null || !cardSource.Keywords.Contains(DanjinCardKeywords.FeiRen))
		{
			return;
		}
		ICombatState combatState = ((PowerModel)this).Owner.CombatState;
		if (combatState != null)
		{
			List<Creature> list = combatState.HittableEnemies.Where((Creature e) => e != target && e.IsAlive).ToList();
			if (list.Count != 0)
			{
				((PowerModel)this).Flash();
				DanjinLog.Verbose($">>>[DanjinMod] 剑扫春秋：绯刃 {((AbstractModel)cardSource).Id.Entry} 命中 {target} ({result.TotalDamage} dmg) → 溅射 {list.Count} 个敌人");
				await CreatureCmd.Damage(choiceContext, (IEnumerable<Creature>)list, (decimal)result.TotalDamage, (ValueProp)4, ((PowerModel)this).Owner);
			}
		}
	}
}
