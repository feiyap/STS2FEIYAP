using System.Collections.Generic;
using System.Threading.Tasks;
using Feiyap.Cards.Ancients;
using Feiyap.Characters;
using Feiyap.Mechanics;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Feiyap.Relics;

public abstract class SwordSaintBase : ModRelicTemplate
{
    protected abstract decimal IaidoDamageMultiplier { get; }

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
        [HoverTipFactory.FromCard(ModelDb.Card<FeiYingYuHuaLuo>())];

    public override async Task AfterObtained()
    {
        await FeiyapQuestRewards.GainAncientCard<FeiYingYuHuaLuo>(Owner);
    }

    public override decimal ModifyDamageMultiplicative(
        Creature? target,
        decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        if (dealer?.Player != Owner || cardSource == null || !FeiyapCardTags.HasIaido(cardSource))
        {
            return amount;
        }

        return amount * IaidoDamageMultiplier;
    }
}

[RegisterRelic(typeof(FeiyapRelicPool))]
public sealed class SwordSaint : SwordSaintBase
{
    protected override decimal IaidoDamageMultiplier => 1.5m;

    public override RelicRarity Rarity => RelicRarity.Rare;

    public override RelicAssetProfile AssetProfile => FeiyapRelicAssets.For(nameof(SwordSaint));
}

[RegisterRelic(typeof(FeiyapRelicPool))]
public sealed class WuMingRen : SwordSaintBase
{
    protected override decimal IaidoDamageMultiplier => 2m;

    public override RelicRarity Rarity => RelicRarity.Rare;

    public override RelicAssetProfile AssetProfile => FeiyapRelicAssets.For(nameof(WuMingRen));
}
