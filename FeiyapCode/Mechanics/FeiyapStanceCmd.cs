using System.Threading.Tasks;
using Feiyap.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Scaffolding.Content;

namespace Feiyap.Mechanics;

/// <summary>
/// 架势互斥：打出新架势时移除其他架势。
/// </summary>
public static class FeiyapStanceCmd
{
    public static async Task ApplyExclusiveStance<TPower>(
        PlayerChoiceContext choiceContext,
        Creature creature,
        decimal amount,
        CardModel? source)
        where TPower : ModPowerTemplate
    {
        await RemoveOtherStances(creature, typeof(TPower));
        await PowerCmd.Apply(
            choiceContext,
            ModelDb.Power<TPower>().ToMutable(),
            creature,
            amount,
            creature,
            source);
    }

    private static async Task RemoveOtherStances(Creature creature, System.Type keep)
    {
        if (keep != typeof(FeiyapTandaoStancePower))
        {
            var power = creature.GetPower<FeiyapTandaoStancePower>();
            if (power != null)
            {
                await PowerCmd.Remove(power);
            }
        }

        if (keep != typeof(FeiyapYindaoStancePower))
        {
            var power = creature.GetPower<FeiyapYindaoStancePower>();
            if (power != null)
            {
                await PowerCmd.Remove(power);
            }
        }

        if (keep != typeof(FeiyapNadaoStancePower))
        {
            var power = creature.GetPower<FeiyapNadaoStancePower>();
            if (power != null)
            {
                await PowerCmd.Remove(power);
            }
        }
    }
}
