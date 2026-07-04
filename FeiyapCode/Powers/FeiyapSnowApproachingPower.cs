using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Feiyap.Powers;

/// <summary>
/// 雪将至：倒计时结束后在回合结束时受到固定伤害。
/// </summary>
[RegisterPower]
public sealed class FeiyapSnowApproachingPower : ModPowerTemplate
{
    public const int DelayTurns = 3;
    public const int DamageAmount = 20;

    public override PowerType Type => PowerType.Debuff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (!participants.Contains(Owner))
        {
            return;
        }

        await PowerCmd.ModifyAmount(choiceContext, this, -1m, Owner, null);
        if (Amount > 0)
        {
            return;
        }

        Flash();
        await CreatureCmd.Damage(
            choiceContext,
            Owner,
            DamageAmount,
            ValueProp.Unblockable | ValueProp.Unpowered,
            null,
            null);
        await PowerCmd.Remove(this);
    }
}
