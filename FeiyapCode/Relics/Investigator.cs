using System.Collections.Generic;
using System.Threading.Tasks;
using Feiyap.Cards.Ancients;
using Feiyap.Characters;
using Feiyap.Mechanics;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Feiyap.Relics;

public abstract class InvestigatorBase : ModRelicTemplate
{
    protected abstract int RegenAmount { get; }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<RegenPower>(RegenAmount)
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromCard(ModelDb.Card<KeXueZheng>()),
        HoverTipFactory.FromPower<RegenPower>()
    ];

    public override async Task AfterObtained()
    {
        if (FeiyapQuestRewards.SuppressQuestRelicObtainEffects)
        {
            return;
        }

        await FeiyapQuestRewards.GainAncientCard<KeXueZheng>(Owner);
    }

    public override async Task BeforeCombatStart()
    {
        Flash();
        await PowerCmd.Apply(
            new ThrowingPlayerChoiceContext(),
            ModelDb.Power<RegenPower>().ToMutable(),
            Owner.Creature,
            RegenAmount,
            Owner.Creature,
            null);
    }
}

[RegisterRelic(typeof(FeiyapRelicPool))]
public sealed class Investigator : InvestigatorBase
{
    protected override int RegenAmount => 3;

    public override RelicRarity Rarity => RelicRarity.Event;

    public override RelicAssetProfile AssetProfile => FeiyapRelicAssets.For(nameof(Investigator));
}

[RegisterRelic(typeof(FeiyapRelicPool))]
public sealed class FeiShengYiWenZi : InvestigatorBase
{
    protected override int RegenAmount => 5;

    public override RelicRarity Rarity => RelicRarity.Event;

    public override RelicAssetProfile AssetProfile => FeiyapRelicAssets.For(nameof(FeiShengYiWenZi));
}
