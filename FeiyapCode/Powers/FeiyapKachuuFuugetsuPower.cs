using System.Collections.Generic;
using System.Threading.Tasks;
using Feiyap.Mechanics;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Feiyap.Powers;

/// <summary>
/// 花鸟风月：打出时与回合开始时获得格挡、居合、活力与残心。
/// </summary>
[RegisterPower]
public sealed class FeiyapKachuuFuugetsuPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player.Creature != Owner || Amount <= 0)
        {
            return;
        }

        await ApplyBundle(choiceContext, null);
    }

    public async Task ApplyOnPlay(PlayerChoiceContext choiceContext, CardModel source)
    {
        if (Amount <= 0)
        {
            return;
        }

        await ApplyBundle(choiceContext, source);
    }

    private async Task ApplyBundle(PlayerChoiceContext choiceContext, CardModel? source)
    {
        Flash();
        await CreatureCmd.GainBlock(Owner, Amount, ValueProp.Move, null);
        await FeiyapIaidoCmd.Gain(choiceContext, Owner, Amount, ValueProp.Move, source, null);
        await PowerCmd.Apply<VigorPower>(choiceContext, Owner, Amount, Owner, source);
        await FeiyapZanxinCmd.Gain(choiceContext, Owner, Amount, source);
    }
}
