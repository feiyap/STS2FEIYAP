using System.Threading.Tasks;
using Feiyap.Characters;
using Feiyap.Mechanics;
using Feiyap.Relics;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;

namespace Feiyap.Cards.Quest;

/// <summary>
/// 流浪：累计体内灼烧伤害任务。
/// </summary>
[RegisterCard(typeof(FeiyapCardPool))]
public sealed class LiuLang : FeiyapQuestCardBase
{
    protected override FeiyapQuestKind QuestKind => FeiyapQuestKind.LiuLang;

    protected override int QuestGoal => 240;

    protected override Task GrantReward(PlayerChoiceContext choiceContext) =>
        FeiyapQuestRewards.GrantQuestRelic<StarSlayer, TianXingJian>(choiceContext, Owner);

    public override Task AfterDamageReceived(
        PlayerChoiceContext choiceContext,
        Creature target,
        DamageResult result,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        if (IsComplete || dealer?.Player != Owner || !FeiyapCardTags.HasInternalBurn(cardSource))
        {
            return Task.CompletedTask;
        }

        if (result.UnblockedDamage > 0)
        {
            AddProgress((int)result.UnblockedDamage);
        }

        return Task.CompletedTask;
    }
}
