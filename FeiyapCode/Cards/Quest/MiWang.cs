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
/// 迷惘：累计攻击牌伤害任务。
/// </summary>
[RegisterCard(typeof(FeiyapCardPool))]
public sealed class MiWang : FeiyapQuestCardBase
{
    protected override FeiyapQuestKind QuestKind => FeiyapQuestKind.MiWang;

    protected override int QuestGoal => 300;

    protected override Task GrantReward(PlayerChoiceContext choiceContext) =>
        FeiyapQuestRewards.GrantQuestRelic<Investigator, FeiShengYiWenZi>(choiceContext, Owner);

    public override Task AfterDamageReceived(
        PlayerChoiceContext choiceContext,
        Creature target,
        DamageResult result,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        // 战斗内会同时订阅牌库本体与战斗克隆，只让牌库本体计进度。
        if (IsCombatClone || dealer?.Player != Owner || cardSource?.Type != CardType.Attack)
        {
            return Task.CompletedTask;
        }

        if (!IsComplete && result.UnblockedDamage > 0)
        {
            AddProgress((int)result.UnblockedDamage);
        }

        return Task.CompletedTask;
    }
}
