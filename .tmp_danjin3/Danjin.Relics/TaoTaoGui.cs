using System.Collections.Generic;
using System.Threading.Tasks;
using Danjin.Powers;
using Danjin.Utils;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.ValueProps;

namespace Danjin.Relics;

public sealed class TaoTaoGui : DanjinRelic
{
	private const decimal ReductionPerShouShi = 0.05m;

	protected override IEnumerable<IHoverTip> AdditionalHoverTips => new _003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.FromPower<ShouShiPower>((int?)null));

	public override RelicRarity Rarity => (RelicRarity)4;

	public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
	{
		if (target != ((RelicModel)this).Owner.Creature)
		{
			return 1m;
		}
		Creature creature = ((RelicModel)this).Owner.Creature;
		int num = ((creature != null) ? creature.GetPowerAmount<ShouShiPower>() : 0);
		if (num <= 0)
		{
			return 1m;
		}
		decimal num2 = 0.05m * (decimal)num;
		if (num2 > 1m)
		{
			num2 = 1m;
		}
		DanjinLog.Verbose($">>>[DanjinMod] 桃桃龟：守势 {num} 层 → 减伤 {num2:P0}");
		return 1m - num2;
	}

	public override Task AfterCombatEnd(CombatRoom _)
	{
		((RelicModel)this).Status = (RelicStatus)0;
		return Task.CompletedTask;
	}
}
