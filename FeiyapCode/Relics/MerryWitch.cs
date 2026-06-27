using System.Collections.Generic;
using System.Threading.Tasks;
using Feiyap.Cards.Ancients;
using Feiyap.Characters;
using Feiyap.Mechanics;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Feiyap.Relics;

public abstract class MerryWitchBase : ModRelicTemplate, IFeiyapIaidoGainMultiplier
{
    protected abstract decimal AlternateBonusMultiplier { get; }

    public override RelicRarity Rarity => RelicRarity.Rare;

    public decimal ModifyIaidoGainMultiplicative(in FeiyapIaidoGainContext context, decimal amount)
    {
        if (context.Creature.Player == Owner && FeiyapCombatTracker.Get(Owner).AlternateBonusActive)
        {
            return amount * AlternateBonusMultiplier;
        }

        return amount;
    }

    public override decimal ModifyDamageMultiplicative(
        Creature? target,
        decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        if (dealer?.Player == Owner && FeiyapCombatTracker.Get(Owner).AlternateBonusActive)
        {
            return amount * AlternateBonusMultiplier;
        }

        return amount;
    }

    public override decimal ModifyBlockMultiplicative(
        Creature target,
        decimal block,
        ValueProp props,
        CardModel? cardSource,
        CardPlay? cardPlay)
    {
        if (target.Player == Owner && FeiyapCombatTracker.Get(Owner).AlternateBonusActive)
        {
            return block * AlternateBonusMultiplier;
        }

        return block;
    }
}

[RegisterRelic(typeof(FeiyapRelicPool))]
public sealed class MerryWitch : MerryWitchBase
{
    protected override decimal AlternateBonusMultiplier => 1.25m;

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
        [HoverTipFactory.FromCard(ModelDb.Card<WorldXxi>())];

    public override RelicAssetProfile AssetProfile => FeiyapRelicAssets.For(nameof(MerryWitch));

    public override async Task AfterObtained()
    {
        await FeiyapQuestRewards.GainAncientCard<WorldXxi>(Owner);
    }
}

[RegisterRelic(typeof(FeiyapRelicPool))]
public sealed class KuangXiaoMoNv : MerryWitchBase
{
    protected override decimal AlternateBonusMultiplier => 1.5m;

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
        [HoverTipFactory.FromCard(ModelDb.Card<WorldXxi>())];

    public override RelicAssetProfile AssetProfile => FeiyapRelicAssets.For(nameof(KuangXiaoMoNv));

    public override async Task AfterObtained()
    {
        await FeiyapQuestRewards.GainAncientCard<WorldXxi>(Owner);
    }
}
