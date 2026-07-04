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
/// 苦修：累计居合伤害任务。
/// </summary>
[RegisterCard(typeof(FeiyapCardPool))]
public sealed class KuXiu : FeiyapQuestCardBase
{
    protected override FeiyapQuestKind QuestKind => FeiyapQuestKind.KuXiu;

    protected override int QuestGoal => 100;

    protected override Task GrantReward(PlayerChoiceContext choiceContext) =>
        FeiyapQuestRewards.GrantQuestRelic<SwordSaint, WuMingRen>(choiceContext, Owner);

    public override Task AfterDamageReceived(
        PlayerChoiceContext choiceContext,
        Creature target,
        DamageResult result,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        // 居合反击伤害由 FeiyapIaidoPower 计入；此处仅统计带居合标签的卡牌伤害。
        // 战斗内会同时订阅牌库本体与战斗克隆，只让牌库本体计进度。
        if (IsCombatClone || dealer?.Player != Owner || !FeiyapCardTags.HasIaido(cardSource))
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
