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
    /// <summary>
    /// 欧罗巴斯之触将任务奖励遗物替换为升级形态时，跳过再次发放先古卡等获得副作用。
    /// </summary>
    internal static bool SuppressQuestRelicObtainEffects { get; set; }

    public static bool HasUpgradedQuestRewards(Player player) =>
        player.Relics.Any(r => r is YuQuYuDuo);

    /// <summary>
    /// 将玩家已持有的基础任务奖励遗物替换为升级形态。
    /// </summary>
    public static async Task UpgradeExistingQuestRelics(Player player)
    {
        SuppressQuestRelicObtainEffects = true;
        try
        {
            await TryUpgradeQuestRelic<Investigator, FeiShengYiWenZi>(player);
            await TryUpgradeQuestRelic<SwordSaint, WuMingRen>(player);
            await TryUpgradeQuestRelic<MerryWitch, KuangXiaoMoNv>(player);
        }
        finally
        {
            SuppressQuestRelicObtainEffects = false;
        }
    }

    private static async Task TryUpgradeQuestRelic<TBase, TUpgraded>(Player player)
        where TBase : RelicModel
        where TUpgraded : RelicModel
    {
        var existing = player.Relics.FirstOrDefault(r => r is TBase);
        if (existing == null)
        {
            return;
        }

        await RelicCmd.Replace(existing, ModelDb.Relic<TUpgraded>().ToMutable());
    }

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

    public static async Task GainAncientCard<TCard>(Player player, bool upgraded = false) where TCard : CardModel
    {
        var card = player.RunState.CreateCard<TCard>(player);
        if (upgraded)
        {
            CardCmd.Upgrade(card);
        }

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
