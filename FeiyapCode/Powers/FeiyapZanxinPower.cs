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
/// 残心：下一次获得的格挡或居合增加等量数值。
/// </summary>
[RegisterPower]
public sealed class FeiyapZanxinPower : ModPowerTemplate, IFeiyapIaidoGainAdditive
{
    private int _pendingBlockBonus;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<string> RegisteredKeywordIds => [FeiyapKeywords.ZanxinId];

    public decimal GetIaidoGainAdditiveBonus(in FeiyapIaidoGainContext context)
    {
        if (context.Creature != Owner || Amount <= 0)
        {
            return 0m;
        }

        return Amount;
    }

    public async ValueTask OnIaidoGainApplied(FeiyapIaidoGainContext context, decimal appliedBonus)
    {
        if (appliedBonus <= 0m)
        {
            return;
        }

        Flash();
        await PowerCmd.Apply(
            context.ChoiceContext,
            this,
            Owner,
            -appliedBonus,
            Owner,
            context.CardSource);
        await FeiyapHermitPower.TryRefundZanxin(context.ChoiceContext, Owner, appliedBonus);
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

        _pendingBlockBonus = Amount;
        return Amount;
    }

    public override async Task AfterBlockGained(Creature creature, decimal amount, ValueProp props, CardModel? cardSource)
    {
        if (creature != Owner || amount <= 0m || _pendingBlockBonus <= 0)
        {
            return;
        }

        var consume = _pendingBlockBonus;
        _pendingBlockBonus = 0;
        Flash();
        await PowerCmd.Apply(new ThrowingPlayerChoiceContext(), this, Owner, -consume, Owner, null);
        await FeiyapHermitPower.TryRefundZanxin(new ThrowingPlayerChoiceContext(), Owner, consume);
    }
}
