using System.Threading.Tasks;
using Feiyap.Mechanics;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Feiyap.Powers;

/// <summary>
/// 剑心犹在：上回合未失去生命时，回合开始获得敏捷。
/// </summary>
[RegisterPower]
public sealed class FeiyapSwordHeartRemainsPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player.Creature != Owner || Amount <= 0)
        {
            return;
        }

        var tracker = FeiyapCombatTracker.Get(player);
        if (tracker.LostHpLastTurn)
        {
            return;
        }

        Flash();
        await PowerCmd.Apply<DexterityPower>(
            choiceContext,
            Owner,
            Amount,
            Owner,
            null);
    }
}
