using System.Collections.Generic;
using System.Threading.Tasks;
using Feiyap.Cards.Ancients;
using Feiyap.Characters;
using Feiyap.Mechanics;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Runs;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Feiyap.Relics;

/// <summary>
/// 拉普拉斯妖：完成全部 3 个转职任务后的隐藏奖励遗物。
/// </summary>
[RegisterRelic(typeof(FeiyapRelicPool))]
public sealed class LaplaceDemon : ModRelicTemplate
{
    public override RelicRarity Rarity => RelicRarity.Event;

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
        [HoverTipFactory.FromCard(ModelDb.Card<WorldShards>())];

    public override RelicAssetProfile AssetProfile => FeiyapRelicAssets.For(nameof(LaplaceDemon));

    public override async Task AfterObtained()
    {
        await FeiyapQuestRewards.GainAncientCard<WorldShards>(Owner);
        await RelicCmd.Obtain<ArchaicTooth>(Owner);
        await RelicCmd.Obtain<TouchOfOrobas>(Owner);
    }

    public override bool TryModifyCardRewardOptionsLate(
        Player player,
        List<CardCreationResult> cardRewards,
        CardCreationOptions options)
    {
        if (player != Owner)
        {
            return false;
        }

        if (options.Flags.HasFlag(CardCreationFlags.NoHookUpgrades))
        {
            return false;
        }

        if (!options.Flags.HasFlag(CardCreationFlags.IsCardReward))
        {
            return false;
        }

        UpgradeAllValidCards(cardRewards);
        return true;
    }

    private void UpgradeAllValidCards(List<CardCreationResult> cardRewards)
    {
        foreach (var cardReward in cardRewards)
        {
            var card = cardReward.Card;
            if (!card.IsUpgradable)
            {
                continue;
            }

            var upgraded = Owner.RunState.CloneCard(card);
            CardCmd.Upgrade(upgraded);
            cardReward.ModifyCard(upgraded, this);
        }
    }
}
