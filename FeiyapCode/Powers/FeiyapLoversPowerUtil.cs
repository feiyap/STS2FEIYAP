using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;

namespace Feiyap.Powers;

internal static class FeiyapLoversPowerUtil
{
    internal static Task DealMirrorDamage(
        PlayerChoiceContext choiceContext,
        Creature target,
        decimal amount,
        Creature? dealer) =>
        CreatureCmd.Damage(
            choiceContext,
            target,
            amount,
            ValueProp.Unpowered | ValueProp.SkipHurtAnim,
            dealer,
            null,
            null);
}
