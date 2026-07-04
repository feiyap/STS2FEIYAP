using Feiyap.Characters;
using Feiyap.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Feiyap.Cards.Rare;

/// <summary>
/// 活杀自在：每获得 1 点居合，获得等量活力（Amount 为倍率）。
/// </summary>
[RegisterCard(typeof(FeiyapCardPool))]
public sealed class FeiyapRare20 : FeiyapCardTemplate
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<FeiyapKassaiJizaiPower>(1m)
    ];

    public FeiyapRare20()
        : base(2, CardType.Power, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "PowerUp", Owner.Character.PowerUpAnimDelay);
        await PowerCmd.Apply(
            choiceContext,
            ModelDb.Power<FeiyapKassaiJizaiPower>().ToMutable(),
            Owner.Creature,
            DynamicVars["FeiyapKassaiJizaiPower"].BaseValue,
            Owner.Creature,
            this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["FeiyapKassaiJizaiPower"].UpgradeValueBy(1m);
    }
}
