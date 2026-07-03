using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;

namespace Danjin.Relics;

public sealed class TeZhiLiaoHuan : DanjinRelic
{
	protected override IEnumerable<IHoverTip> AdditionalHoverTips => new _003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.FromPower<ZhuShiZhiKePower>((int?)null));

	public override RelicRarity Rarity => (RelicRarity)4;

	public override Task AfterCombatEnd(CombatRoom _)
	{
		((RelicModel)this).Status = (RelicStatus)0;
		return Task.CompletedTask;
	}
}
