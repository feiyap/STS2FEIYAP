using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Feiyap.Powers;

/// <summary>
/// 黑洞：获得负面效果时，额外获得 1 层。
/// </summary>
[RegisterPower]
public sealed class FeiyapVoidHolePower : ModPowerTemplate
{
    private bool _subscribed;

    public override PowerType Type => PowerType.Debuff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        if (_subscribed)
        {
            return;
        }

        Owner.PowerApplied += OnOwnerPowerApplied;
        _subscribed = true;
        await Task.CompletedTask;
    }

    public override Task AfterRemoved(Creature oldOwner)
    {
        if (_subscribed)
        {
            Owner.PowerApplied -= OnOwnerPowerApplied;
            _subscribed = false;
        }

        return Task.CompletedTask;
    }

    private void OnOwnerPowerApplied(PowerModel power)
    {
        if (power.Type != PowerType.Debuff || power == this)
        {
            return;
        }

        TaskHelper.RunSafely(PowerCmd.Apply(
            new ThrowingPlayerChoiceContext(),
            this,
            Owner,
            1m,
            Owner,
            null));
    }
}
