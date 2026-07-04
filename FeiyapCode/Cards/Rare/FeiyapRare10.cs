using Feiyap.Characters;
using Feiyap.Mechanics;
using Feiyap.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Scaffolding.Content;

namespace Feiyap.Cards.Rare;

/// <summary>
/// 幾星霜：获得居合；每次抽到本场战斗居合获得量增加。
/// </summary>
[RegisterCard(typeof(FeiyapCardPool))]
public sealed class FeiyapRare10 : FeiyapCardTemplate
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [FeiyapKeywords.Iaido];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new IaidoVar(8m, ValueProp.Move),
        new DynamicVar("DrawBonus", 4m)
    ];

    public FeiyapRare10()
        : base(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }

    public override Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
    {
        if (card != this || Owner == null)
        {
            return Task.CompletedTask;
        }

        FeiyapCombatTracker.Get(Owner).IaidoGainCombatBonus += DynamicVars["DrawBonus"].BaseValue;
        return Task.CompletedTask;
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
    }

    protected override void OnUpgrade()
    {
        DynamicVars[IaidoVar.DefaultName].UpgradeValueBy(2m);
        DynamicVars["DrawBonus"].UpgradeValueBy(2m);
    }
}
