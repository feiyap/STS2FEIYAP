using Feiyap.Characters;
using Feiyap.Mechanics;
using Feiyap.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Scaffolding.Content;

namespace Feiyap.Cards.Rare;

/// <summary>
/// 一期一会：获得 8 / 12 点居合与 4 / 6 层居合强化，并将居合保留至下回合。
/// </summary>
[RegisterCard(typeof(FeiyapCardPool))]
public sealed class FeiyapRare11 : FeiyapCardTemplate
{
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        FeiyapKeywords.Iaido,
        FeiyapKeywords.IaidoEnhance
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new IaidoVar(8m, ValueProp.Move),
        new PowerVar<FeiyapIaidoEnhancePower>(4m)
    ];

    public FeiyapRare11()
        : base(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await FeiyapIaidoCmd.Gain(
            choiceContext,
            Owner.Creature,
            DynamicVars[IaidoVar.DefaultName].BaseValue,
            ValueProp.Move,
            this,
            cardPlay);

        await PowerCmd.Apply<FeiyapIaidoEnhancePower>(
            choiceContext,
            Owner.Creature,
            DynamicVars["FeiyapIaidoEnhancePower"].BaseValue,
            Owner.Creature,
            this);

        FeiyapCombatTracker.Get(Owner).RetainIaidoNextTurn = true;
    }

    protected override void OnUpgrade()
    {
        DynamicVars[IaidoVar.DefaultName].UpgradeValueBy(4m);
        DynamicVars["FeiyapIaidoEnhancePower"].UpgradeValueBy(2m);
    }
}
