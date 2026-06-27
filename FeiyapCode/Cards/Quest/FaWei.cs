using System.Threading.Tasks;
using Feiyap.Characters;
using Feiyap.Mechanics;
using Feiyap.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;

namespace Feiyap.Cards.Quest;

/// <summary>
/// 乏味：交替使用攻击/技能任务。
/// </summary>
[RegisterCard(typeof(FeiyapCardPool))]
public sealed class FaWei : FeiyapQuestCardBase
{
    protected override FeiyapQuestKind QuestKind => FeiyapQuestKind.FaWei;

    protected override int QuestGoal => 100;

    protected override Task GrantReward(PlayerChoiceContext choiceContext) =>
        FeiyapQuestRewards.GrantQuestRelic<MerryWitch, KuangXiaoMoNv>(choiceContext, Owner);
}
