using Feiyap.Characters;
using Feiyap.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Feiyap.Cards.Rare;

/// <summary>
/// 立处皆真：无负面能力时造成的伤害提升。
/// </summary>
[RegisterCard(typeof(FeiyapCardPool))]
public sealed class FeiyapRare19 : FeiyapCardTemplate
{
    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
        [HoverTipFactory.FromPower<FeiyapStandingTruthPower>()];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<FeiyapStandingTruthPower>(50m)
    ];

    public FeiyapRare19()
        : base(1, CardType.Power, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "PowerUp", Owner.Character.PowerUpAnimDelay);
        await PowerCmd.Apply(
            choiceContext,
            ModelDb.Power<FeiyapStandingTruthPower>().ToMutable(),
            Owner.Creature,
            DynamicVars["FeiyapStandingTruthPower"].BaseValue,
            Owner.Creature,
            this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["FeiyapStandingTruthPower"].UpgradeValueBy(50m);
    }
}
