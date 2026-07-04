using System.Threading.Tasks;
using Feiyap.Mechanics;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Feiyap.Powers;

/// <summary>
/// VI-恋人：正位反弹自身受到的伤害，逆位反弹目标受到的伤害。
/// </summary>
[RegisterPower]
public sealed class FeiyapLoversPower : ModPowerTemplate
{
    private Creature? _markedTarget;
    private bool _isReversed;
    private bool _dualEffect;
    private bool _uprightConsumed;
    private bool _reversedConsumed;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    public void Configure(Creature target, bool reversed)
    {
        AssertMutable();
        _markedTarget = target;
        _isReversed = reversed;
    }

    public void SetDualEffect(bool dual)
    {
        AssertMutable();
        _dualEffect = dual;
    }

    public override async Task BeforeDamageReceived(
        PlayerChoiceContext choiceContext,
        Creature target,
        decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        if (amount <= 0m || _markedTarget == null || !props.IsPoweredAttack())
        {
            return;
        }

        if (_dualEffect)
        {
            if (!_uprightConsumed && target == Owner && dealer != null && dealer.Side != Owner.Side)
            {
                _uprightConsumed = true;
                Flash();
                await DealMirrorDamage(choiceContext, _markedTarget, amount);
            }

            if (!_reversedConsumed && target == _markedTarget && dealer != null)
            {
                _reversedConsumed = true;
                Flash();
                await DealMirrorDamage(choiceContext, _markedTarget, amount);
            }

            if (_uprightConsumed && _reversedConsumed)
            {
                await PowerCmd.Remove(this);
            }

            return;
        }

        if (_uprightConsumed && _reversedConsumed)
        {
            return;
        }

        if (!_isReversed && target == Owner && dealer != null && dealer.Side != Owner.Side)
        {
            _uprightConsumed = true;
            Flash();
            await DealMirrorDamage(choiceContext, _markedTarget, amount);
            await PowerCmd.Remove(this);
            return;
        }

        if (_isReversed && target == _markedTarget && dealer != null)
        {
            _reversedConsumed = true;
            Flash();
            await DealMirrorDamage(choiceContext, _markedTarget, amount);
            await PowerCmd.Remove(this);
        }
    }

    private static Task DealMirrorDamage(PlayerChoiceContext choiceContext, Creature target, decimal amount) =>
        CreatureCmd.Damage(
            choiceContext,
            target,
            amount,
            ValueProp.Unpowered | ValueProp.SkipHurtAnim,
            null,
            null);
}
