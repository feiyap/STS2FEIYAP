using System.Collections.Generic;
using Feiyap.Mechanics;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Feiyap.Powers;

/// <summary>
/// 残心：下一次打出的卡牌，其获得的格挡与居合增加等量数值。
/// </summary>
[RegisterPower]
public sealed class FeiyapZanxinPower : ModPowerTemplate, IFeiyapIaidoGainAdditive
{
    private CardModel? _deferredCardSource;
    private int _deferredConsumeAmount;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => FeiyapPowerAssets.For(nameof(FeiyapZanxinPower));

    protected override IEnumerable<string> RegisteredKeywordIds => [FeiyapKeywords.ZanxinId];

    public decimal GetIaidoGainAdditiveBonus(in FeiyapIaidoGainContext context)
    {
        if (context.Creature != Owner || Amount <= 0)
        {
            return 0m;
        }

        if (!IsOwnCardPlay(context.CardSource, context.CardPlay))
        {
            return 0m;
        }

        EnsureDeferredConsumption(context.CardSource!, Amount);
        return Amount;
    }

    public ValueTask OnIaidoGainApplied(FeiyapIaidoGainContext context, decimal appliedBonus)
    {
        if (appliedBonus <= 0m || !IsOwnCardPlay(context.CardSource, context.CardPlay))
        {
            return ValueTask.CompletedTask;
        }

        Flash();
        return ValueTask.CompletedTask;
    }

    public override decimal ModifyBlockAdditive(
        Creature target,
        decimal block,
        ValueProp props,
        CardModel? cardSource,
        CardPlay? cardPlay)
    {
        if (target != Owner || block <= 0m || Amount <= 0 || !props.HasFlag(ValueProp.Move))
        {
            return 0m;
        }

        if (!IsOwnCardPlay(cardSource, cardPlay))
        {
            return 0m;
        }

        EnsureDeferredConsumption(cardSource!, Amount);
        return Amount;
    }

    public override async Task AfterCardPlayedLate(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (_deferredCardSource == null || _deferredConsumeAmount <= 0)
        {
            return;
        }

        if (cardPlay.Card != _deferredCardSource)
        {
            return;
        }

        var consume = _deferredConsumeAmount;
        ClearDeferredConsumption();
        await ConsumeAmountAsync(choiceContext, consume);
    }

    private bool IsOwnCardPlay(CardModel? cardSource, CardPlay? cardPlay) =>
        cardPlay != null
        && cardSource != null
        && cardSource.Owner?.Creature == Owner;

    private void EnsureDeferredConsumption(CardModel cardSource, int amount)
    {
        if (_deferredCardSource == null)
        {
            _deferredCardSource = cardSource;
            _deferredConsumeAmount = amount;
            return;
        }

        if (_deferredCardSource == cardSource)
        {
            _deferredConsumeAmount = Math.Max(_deferredConsumeAmount, amount);
        }
    }

    private void ClearDeferredConsumption()
    {
        _deferredCardSource = null;
        _deferredConsumeAmount = 0;
    }

    private async Task ConsumeAmountAsync(PlayerChoiceContext choiceContext, decimal consume)
    {
        if (consume <= 0m)
        {
            return;
        }

        await PowerCmd.Apply(choiceContext, this, Owner, -consume, Owner, null);
        await FeiyapHermitReversedPower.TryRefundZanxin(choiceContext, Owner, consume);
    }
}
