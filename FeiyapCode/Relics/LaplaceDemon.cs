using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Feiyap.Characters;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Relics;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Feiyap.Relics;

/// <summary>
/// 拉普拉斯妖：完成全部 3 个转职任务后的隐藏奖励遗物。
/// </summary>
[RegisterRelic(typeof(FeiyapRelicPool))]
public sealed class LaplaceDemon : ModRelicTemplate, IFeiyapHiddenFromRelicCompendium
{
    public override RelicRarity Rarity => RelicRarity.Event;

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        ..HoverTipFactory.FromRelic<ArchaicTooth>(),
        ..HoverTipFactory.FromRelic<TouchOfOrobas>()
    ];

    public override RelicAssetProfile AssetProfile => FeiyapRelicAssets.For(nameof(LaplaceDemon));

    public override async Task AfterObtained()
    {
        // 已持有时不再发放，避免古老牙齿二次 AfterObtained 时 StarterCard/AncientCard 为空导致 NRE
        if (!Owner.Relics.Any(static r => r is ArchaicTooth))
        {
            await RelicCmd.Obtain<ArchaicTooth>(Owner);
        }

        if (!Owner.Relics.Any(static r => r is TouchOfOrobas))
        {
            await RelicCmd.Obtain<TouchOfOrobas>(Owner);
        }
    }
}
