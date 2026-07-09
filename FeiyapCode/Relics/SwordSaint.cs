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
    /// <summary>完美居合伤害提升百分比（如 500 表示 +500%）。</summary>
    internal abstract decimal PerfectIaidoDamageBonusPercent { get; }

    protected override IEnumerable<string> RegisteredKeywordIds => [FeiyapKeywords.PerfectIaidoId];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
        [HoverTipFactory.FromCard(ModelDb.Card<FeiYingYuHuaLuo>())];

    public override async Task AfterObtained()
    {
        if (FeiyapQuestRewards.SuppressQuestRelicObtainEffects)
        {
            return;
        }

        await FeiyapQuestRewards.GainAncientCard<FeiYingYuHuaLuo>(Owner);
    }
}

[RegisterRelic(typeof(FeiyapRelicPool))]
public sealed class SwordSaint : SwordSaintBase
{
    internal override decimal PerfectIaidoDamageBonusPercent => 500m;

    public override RelicRarity Rarity => RelicRarity.Event;

    public override RelicAssetProfile AssetProfile => FeiyapRelicAssets.For(nameof(SwordSaint));
}

[RegisterRelic(typeof(FeiyapRelicPool))]
public sealed class WuMingRen : SwordSaintBase
{
    internal override decimal PerfectIaidoDamageBonusPercent => 1000m;

    public override RelicRarity Rarity => RelicRarity.Event;

    public override RelicAssetProfile AssetProfile => FeiyapRelicAssets.For(nameof(WuMingRen));
}
