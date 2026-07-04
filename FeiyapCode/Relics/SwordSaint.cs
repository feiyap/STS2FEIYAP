using System.Collections.Generic;
using System.Threading.Tasks;
using Feiyap.Cards.Ancients;
using Feiyap.Characters;
using Feiyap.Mechanics;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Feiyap.Relics;

public abstract class SwordSaintBase : ModRelicTemplate
{
    internal abstract decimal PerfectIaidoDamageMultiplier { get; }

    protected override IEnumerable<string> RegisteredKeywordIds => [FeiyapKeywords.PerfectIaidoId];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
        [HoverTipFactory.FromCard(ModelDb.Card<FeiYingYuHuaLuo>())];

    public override async Task AfterObtained()
    {
        await FeiyapQuestRewards.GainAncientCard<FeiYingYuHuaLuo>(Owner);
    }
}

[RegisterRelic(typeof(FeiyapRelicPool))]
public sealed class SwordSaint : SwordSaintBase
{
    internal override decimal PerfectIaidoDamageMultiplier => 10m;

    public override RelicRarity Rarity => RelicRarity.Rare;

    public override RelicAssetProfile AssetProfile => FeiyapRelicAssets.For(nameof(SwordSaint));
}

[RegisterRelic(typeof(FeiyapRelicPool))]
public sealed class WuMingRen : SwordSaintBase
{
    internal override decimal PerfectIaidoDamageMultiplier => 20m;

    public override RelicRarity Rarity => RelicRarity.Rare;

    public override RelicAssetProfile AssetProfile => FeiyapRelicAssets.For(nameof(WuMingRen));
}
