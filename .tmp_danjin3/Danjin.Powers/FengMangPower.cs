using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Danjin.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Danjin.Powers;

public sealed class FengMangPower : DanjinPower
{
	private CardModel? _attachedCard;

	public override PowerType Type => (PowerType)1;

	public override PowerStackType StackType => (PowerStackType)1;

	protected override IEnumerable<IHoverTip> AdditionalHoverTips => new _003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.FromPower<ChuXuePower>((int?)null));

	public override Task BeforeCardPlayed(CardPlay cardPlay)
	{
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Invalid comparison between Unknown and I4
		if (_attachedCard != null)
		{
			return Task.CompletedTask;
		}
		if (cardPlay.Card == null)
		{
			return Task.CompletedTask;
		}
		Player owner = cardPlay.Card.Owner;
		if (((owner != null) ? owner.Creature : null) != ((PowerModel)this).Owner)
		{
			return Task.CompletedTask;
		}
		if (cardPlay.IsAutoPlay)
		{
			return Task.CompletedTask;
		}
		if ((int)cardPlay.Card.Type != 1)
		{
			return Task.CompletedTask;
		}
		_attachedCard = cardPlay.Card;
		DanjinLog.Verbose($">>>[DanjinMod] 锋芒({((PowerModel)this).Amount} 层)：锁定下一张牌 [{cardPlay.Card.Title}]");
		return Task.CompletedTask;
	}

	public override async Task AfterDamageGiven(PlayerChoiceContext choiceContext, Creature? dealer, DamageResult result, ValueProp props, Creature target, CardModel? cardSource)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		if (_attachedCard != null && cardSource == _attachedCard && dealer == ((PowerModel)this).Owner && target != ((PowerModel)this).Owner && target.IsAlive && ValuePropExtensions.IsPoweredAttack(props) && ((PowerModel)this).Amount > 0)
		{
			((PowerModel)this).Flash();
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(31, 3);
			defaultInterpolatedStringHandler.AppendLiteral(">>>[DanjinMod] 锋芒：[");
			defaultInterpolatedStringHandler.AppendFormatted((cardSource != null) ? cardSource.Title : null);
			defaultInterpolatedStringHandler.AppendLiteral("] 对 ");
			defaultInterpolatedStringHandler.AppendFormatted<Creature>(target);
			defaultInterpolatedStringHandler.AppendLiteral(" 附加 ");
			defaultInterpolatedStringHandler.AppendFormatted(((PowerModel)this).Amount);
			defaultInterpolatedStringHandler.AppendLiteral(" 层出血");
			DanjinLog.Verbose(defaultInterpolatedStringHandler.ToStringAndClear());
			await PowerCmd.Apply<ChuXuePower>(choiceContext, target, (decimal)((PowerModel)this).Amount, ((PowerModel)this).Owner, cardSource, false);
		}
	}

	public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		if (_attachedCard != null && cardPlay.Card == _attachedCard && cardPlay.IsLastInSeries)
		{
			DanjinLog.Verbose(">>>[DanjinMod] 锋芒：[" + cardPlay.Card.Title + "] 播放完毕，消耗锋芒");
			_attachedCard = null;
			await PowerCmd.Remove((PowerModel)(object)this);
		}
	}
}
