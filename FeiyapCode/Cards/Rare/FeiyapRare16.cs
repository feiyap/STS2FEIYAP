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
/// 花鸟风月：打出时与回合开始时获得格挡、居合、活力与残心。
/// </summary>
[RegisterCard(typeof(FeiyapCardPool))]
public sealed class FeiyapRare16 : FeiyapCardTemplate
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<FeiyapKachuuFuugetsuPower>(4m)
    ];

    public FeiyapRare16()
        : base(3, CardType.Power, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "PowerUp", Owner.Character.PowerUpAnimDelay);
        var power = (FeiyapKachuuFuugetsuPower)ModelDb.Power<FeiyapKachuuFuugetsuPower>().ToMutable();
        await PowerCmd.Apply(
            choiceContext,
            power,
            Owner.Creature,
            DynamicVars["FeiyapKachuuFuugetsuPower"].BaseValue,
            Owner.Creature,
            this);
        await power.ApplyOnPlay(choiceContext, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["FeiyapKachuuFuugetsuPower"].UpgradeValueBy(1m);
    }
}
