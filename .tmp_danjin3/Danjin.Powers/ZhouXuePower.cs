using System.Collections.Generic;
using System.Threading.Tasks;
using Danjin.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Danjin.Powers;

public sealed class ZhouXuePower : DanjinPower
{
	private const int BleedPerAttack = 2;

	public const decimal BleedDamageMultiplier = 1.5m;

	public const decimal HealMultiplier = 1m;

	public override PowerType Type => (PowerType)1;

	public override PowerStackType StackType => (PowerStackType)2;

	protected override IEnumerable<IHoverTip> AdditionalHoverTips => new _003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.FromPower<ChuXuePower>((int?)null));

	public override async Task AfterDamageGiven(PlayerChoiceContext choiceContext, Creature? dealer, DamageResult result, ValueProp props, Creature target, CardModel? cardSource)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		if (dealer == ((PowerModel)this).Owner && target.IsEnemy && target.IsAlive && ValuePropExtensions.IsPoweredAttack(props))
		{
			((PowerModel)this).Flash();
			DanjinLog.Verbose($">>>[DanjinMod] 咒血：对 {target} 施加 {2} 层出血(攻击触发)");
			await PowerCmd.Apply<ChuXuePower>(choiceContext, target, 2m, ((PowerModel)this).Owner, cardSource, false);
		}
	}
}
