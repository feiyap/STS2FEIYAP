using System.Collections.Generic;
using System.Threading.Tasks;
using Feiyap.Cards.Ancients;
using Feiyap.Characters;
using Feiyap.Mechanics;
using Feiyap.Powers;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Feiyap.Relics;

public abstract class MerryWitchBase : ModRelicTemplate
{
    protected abstract decimal ResourceAmount { get; }

    public override RelicRarity Rarity => RelicRarity.Event;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<FeiyapZanxinPower>(ResourceAmount),
        new PowerVar<VigorPower>(ResourceAmount)
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromCard(ModelDb.Card<WorldXxi>()),
        HoverTipFactory.FromPower<FeiyapZanxinPower>(),
        HoverTipFactory.FromPower<VigorPower>()
    ];

    public override async Task AfterObtained()
    {
        if (FeiyapQuestRewards.SuppressQuestRelicObtainEffects)
        {
            return;
        }

        await FeiyapQuestRewards.GainAncientCard<WorldXxi>(Owner, this is KuangXiaoMoNv);
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (!CombatManager.Instance.IsInProgress || cardPlay.Card.Owner != Owner)
        {
            return;
        }

        if (cardPlay.Card.Type == CardType.Attack)
        {
            Flash();
            await FeiyapZanxinCmd.Gain(
                choiceContext,
                Owner.Creature,
                ResourceAmount,
                null);
            return;
        }

        if (cardPlay.Card.Type == CardType.Skill)
        {
            Flash();
            await PowerCmd.Apply<VigorPower>(
                choiceContext,
                Owner.Creature,
                ResourceAmount,
                Owner.Creature,
                null);
        }
    }
}

[RegisterRelic(typeof(FeiyapRelicPool))]
public sealed class MerryWitch : MerryWitchBase
{
    protected override decimal ResourceAmount => 1m;

    public override RelicAssetProfile AssetProfile => FeiyapRelicAssets.For(nameof(MerryWitch));
}

[RegisterRelic(typeof(FeiyapRelicPool))]
public sealed class KuangXiaoMoNv : MerryWitchBase
{
    protected override decimal ResourceAmount => 2m;

    public override RelicAssetProfile AssetProfile => FeiyapRelicAssets.For(nameof(KuangXiaoMoNv));
}
