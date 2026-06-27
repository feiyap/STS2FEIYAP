using System.Collections.Generic;
using System.Threading.Tasks;
using Feiyap.Mechanics;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Feiyap.Powers;

/// <summary>
/// 热寂：回合开始时失去 1 点生命；回合结束时层数翻倍。
/// </summary>
[RegisterPower]
public sealed class FeiyapHeatDeathPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Debuff;

    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<string> RegisteredKeywordIds => [FeiyapKeywords.HeatDeathId];

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player.Creature != Owner || Amount <= 0)
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

        Flash();
        await PowerCmd.Apply(choiceContext, this, Owner, Amount, Owner, null);
    }
}
