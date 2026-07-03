using System.Collections.Generic;
using System.Threading.Tasks;
using Danjin.Resources;
using Danjin.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;

namespace Danjin.Relics;

public sealed class YunDingLongXuSu : DanjinRelic
{
	private const int StarsGainedPerTurn = 1;

	public override RelicRarity Rarity => (RelicRarity)4;

	public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		Player owner = ((RelicModel)this).Owner;
		if (((owner != null) ? owner.Creature : null) != null && side == ((RelicModel)this).Owner.Creature.Side && CombatManager.Instance.IsInProgress)
		{
			((RelicModel)this).Flash();
			DanjinLog.Verbose($">>>[DanjinMod] 云顶龙须酥: 回合结束, 获得 {1} 点彤华");
			await TonghuaCmd.GainTonghua(1m, ((RelicModel)this).Owner);
		}
	}

	public override Task AfterCombatEnd(CombatRoom _)
	{
		((RelicModel)this).Status = (RelicStatus)0;
		return Task.CompletedTask;
	}
}
