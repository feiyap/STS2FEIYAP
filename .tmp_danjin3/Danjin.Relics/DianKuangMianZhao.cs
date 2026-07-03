using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Danjin.Powers;
using Danjin.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;

namespace Danjin.Relics;

public sealed class DianKuangMianZhao : DanjinRelic
{
	private const int InitialBleedStacks = 5;

	protected override IEnumerable<IHoverTip> AdditionalHoverTips => new _003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.FromPower<ChuXuePower>((int?)null));

	public override RelicRarity Rarity => (RelicRarity)3;

	public override async Task BeforeCombatStart()
	{
		Player owner = ((RelicModel)this).Owner;
		Creature val = ((owner != null) ? owner.Creature : null);
		if (val == null)
		{
			return;
		}
		ICombatState combatState = val.CombatState;
		if (combatState != null)
		{
			List<Creature> list = combatState.HittableEnemies.Where((Creature e) => e.IsAlive).ToList();
			if (list.Count == 0)
			{
				DanjinLog.Verbose(">>>[DanjinMod] 癫狂面罩: 战斗开始时没有可命中敌人, 跳过");
				return;
			}
			DanjinLog.Verbose($">>>[DanjinMod] 癫狂面罩: 对 {list.Count} 个敌人施加 {5} 层出血");
			((RelicModel)this).Flash();
			await PowerCmd.Apply<ChuXuePower>((PlayerChoiceContext)new BlockingPlayerChoiceContext(), (IEnumerable<Creature>)list, 5m, val, (CardModel)null, false);
		}
	}

	public override Task AfterCombatEnd(CombatRoom _)
	{
		((RelicModel)this).Status = (RelicStatus)0;
		return Task.CompletedTask;
	}
}
