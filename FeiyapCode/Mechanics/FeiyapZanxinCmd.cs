using Feiyap.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace Feiyap.Mechanics;

/// <summary>
/// 残心层数的获得。
/// </summary>
public static class FeiyapZanxinCmd
{
    public static async Task Gain(
        PlayerChoiceContext choiceContext,
        Creature creature,
        decimal amount,
        CardModel? cardSource)
    {
        if (creature == null || amount <= 0m)
        {
            return;
        }

        await PowerCmd.Apply(
            choiceContext,
            ModelDb.Power<FeiyapZanxinPower>().ToMutable(),
            creature,
            amount,
            creature,
            cardSource);
    }
}
