using System.Threading.Tasks;
using Feiyap.Mechanics;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Feiyap.Powers;

/// <summary>
/// IX-隐者：正位消耗活力时返还，逆位消耗残心时返还。
/// </summary>
[RegisterPower]
public sealed class FeiyapHermitPower : ModPowerTemplate
{
    private bool _isReversed;
    private bool _dualEffect;
    private bool _uprightConsumed;
    private bool _reversedConsumed;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    public bool IsReversed => _isReversed;

    public bool IsDualEffect => _dualEffect;

    public bool IsUprightConsumed => _uprightConsumed;

    public bool IsReversedConsumed => _reversedConsumed;

    public void SetReversed(bool reversed)
    {
        AssertMutable();
        _isReversed = reversed;
    }

    public void SetDualEffect(bool dual)
    {
        AssertMutable();
        _dualEffect = dual;
    }

    public void MarkUprightConsumed()
    {
        AssertMutable();
        _uprightConsumed = true;
    }

    public void MarkReversedConsumed()
    {
        AssertMutable();
        _reversedConsumed = true;
    }

    public bool IsFullyConsumed() =>
        _dualEffect ? _uprightConsumed && _reversedConsumed : _uprightConsumed || _reversedConsumed;

    public static async Task TryRefundZanxin(
        PlayerChoiceContext choiceContext,
        Creature owner,
        decimal consumed)
    {
        if (consumed <= 0m)
        {
            return;
        }

        var hermit = owner.GetPower<FeiyapHermitPower>();
        if (hermit == null || hermit.IsReversedConsumed)
        {
            return;
        }

        if (!hermit.IsDualEffect && !hermit.IsReversed)
        {
            return;
        }

        hermit.MarkReversedConsumed();
        hermit.Flash();
        await FeiyapZanxinCmd.Gain(choiceContext, owner, consumed, null);
        if (hermit.IsFullyConsumed())
        {
            await PowerCmd.Remove(hermit);
        }
    }
}
