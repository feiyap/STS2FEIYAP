using System.Collections.Generic;
using System.Threading.Tasks;
using Danjin.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;

namespace Danjin.Powers;

public sealed class KangNingXuePower : DanjinPower
{
	private const int BleedPerTurnEnd = 5;

	public override PowerType Type => (PowerType)2;

	public override PowerStackType StackType => (PowerStackType)2;

	protected override IEnumerable<IHoverTip> AdditionalHoverTips => new _003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.FromPower<ChuXuePower>((int?)null));

	public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		if (side == ((PowerModel)this).Owner.Side && ((PowerModel)this).Owner.IsAlive)
		{
			DanjinLog.Verbose($">>>[DanjinMod] 抗凝血：{((PowerModel)this).Owner} 回合结束，自动获得 {5} 层出血");
			((PowerModel)this).Flash();
			await PowerCmd.Apply<ChuXuePower>(choiceContext, ((PowerModel)this).Owner, 5m, ((PowerModel)this).Applier, (CardModel)null, false);
		}
	}
}
