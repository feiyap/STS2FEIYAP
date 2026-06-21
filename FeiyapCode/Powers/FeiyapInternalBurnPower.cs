using System.Collections.Generic;
using System.Threading.Tasks;
using Feiyap.Mechanics;
using MegaCrit.Sts2.Core.Combat;
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
/// 体内灼烧：攻击时失去 1 点生命；回合结束时减少 1 层。
/// </summary>
[RegisterPower]
public sealed class FeiyapInternalBurnPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Debuff;

    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<string> RegisteredKeywordIds => [FeiyapKeywords.InternalBurnId];

    public override async Task AfterDamageGiven(
        PlayerChoiceContext choiceContext,
        Creature? dealer,
        DamageResult result,
        ValueProp props,
        Creature target,
        CardModel? cardSource)
    {
        if (dealer != Owner || Amount <= 0 || !IsPoweredAttack(props, cardSource))
        {
            return;
        }

        Flash();
        await CreatureCmd.Damage(
            choiceContext,
            Owner,
            1m,
            ValueProp.Unblockable | ValueProp.Unpowered,
            null,
            null);
    }

    public override async Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (!participants.Contains(Owner) || Amount <= 0)
        {
            return;
        }

        await PowerCmd.Decrement(this);
    }

    private static bool IsPoweredAttack(ValueProp props, CardModel? cardSource)
    {
        if (props.HasFlag(ValueProp.Unpowered))
        {
            return false;
        }

        if (cardSource?.Type == CardType.Attack)
        {
            return true;
        }

        return props.HasFlag(ValueProp.Move);
    }
}
