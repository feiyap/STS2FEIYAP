using System.Collections.Generic;
using System.Threading.Tasks;
using Danjin.Powers;
using Danjin.Resources;
using Danjin.Utils;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;

namespace Danjin.Relics;

public sealed class HuaiJinZhiYu : DanjinRelic
{
	private const int OpeningAttackStance = 1;

	protected override IEnumerable<IHoverTip> AdditionalHoverTips => new _003C_003Ez__ReadOnlyArray<IHoverTip>((IHoverTip[])(object)new IHoverTip[2]
	{
		HoverTipFactory.FromPower<GongShiPower>((int?)null),
		HoverTipFactory.FromPower<ShouShiPower>((int?)null)
	});

	public override RelicRarity Rarity => (RelicRarity)1;

	public override async Task BeforeCombatStart()
	{
		((RelicModel)this).Flash();
		DanjinLog.Verbose($">>>[DanjinMod] 怀瑾之玉：战斗开始 → 开局获得 {1} 层攻势");
		await StanceCmd.GainAttackStance((PlayerChoiceContext)new BlockingPlayerChoiceContext(), ((RelicModel)this).Owner);
	}

	public override Task AfterCombatEnd(CombatRoom _)
	{
		((RelicModel)this).Status = (RelicStatus)0;
		return Task.CompletedTask;
	}
}
