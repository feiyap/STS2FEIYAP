using System.Linq;
using System.Threading.Tasks;
using Feiyap.Relics;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace Feiyap.Mechanics;

/// <summary>
/// 转职任务奖励发放辅助。
/// </summary>
public static class FeiyapQuestRewards
{
    public static bool HasUpgradedQuestRewards(Player player) =>
        player.Relics.Any(r => r is YuQuYuDuo);

    public static async Task GrantQuestRelic<TBase, TUpgraded>(PlayerChoiceContext context, Player player)
        where TBase : RelicModel
        where TUpgraded : RelicModel
    {
        if (HasUpgradedQuestRewards(player))
        {
            await RelicCmd.Obtain<TUpgraded>(player);
        }
        else
        {
            await RelicCmd.Obtain<TBase>(player);
        }
    }

    public static async Task GainAncientCard<TCard>(Player player) where TCard : CardModel
    {
        var card = player.RunState.CreateCard<TCard>(player);
        CardCmd.Upgrade(card);
        CardCmd.PreviewCardPileAdd(await CardPileCmd.Add(card, PileType.Deck), 2f);
    }

    public static void MarkQuestCompleted(Player player, FeiyapQuestKind kind)
    {
        var yuQu = player.Relics.FirstOrDefault(r => r is YuQuBase) as YuQuBase;
        yuQu?.MarkQuestCompleted(kind);
    }

    public static async Task TryGrantLaplaceDemon(Player player)
    {
        var yuQu = player.Relics.FirstOrDefault(r => r is YuQuBase) as YuQuBase;
        if (yuQu == null || !yuQu.HasCompletedAllQuests)
        {
            return;
        }

        if (player.Relics.Any(r => r is LaplaceDemon))
        {
            return;
        }

        await RelicCmd.Obtain<LaplaceDemon>(player);
    }
}
