using Feiyap.Characters;
using Feiyap.Mechanics;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Scaffolding.Content;

namespace Feiyap.Cards.Rare;

/// <summary>
/// 幾星霜：获得居合；每次抽到时，仅增加本张牌本场战斗的居合获得量。
/// </summary>
[RegisterCard(typeof(FeiyapCardPool))]
public sealed class FeiyapRare10 : FeiyapCardTemplate
{
    private decimal _combatDrawBonus;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [FeiyapKeywords.Iaido];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new IaidoVar(4m, ValueProp.Move),
        new DynamicVar("DrawBonus", 4m)
    ];

    public FeiyapRare10()
        : base(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }

    public override Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
    {
        if (card != this)
        {
            return Task.CompletedTask;
        }

        AssertMutable();
        var bonus = DynamicVars["DrawBonus"].BaseValue;
        DynamicVars[IaidoVar.DefaultName].BaseValue += bonus;
        _combatDrawBonus += bonus;
        return Task.CompletedTask;
    }

    public override Task BeforeCombatStart()
    {
        ResetCombatDrawBonus();
        return Task.CompletedTask;
    }

    public override Task AfterCombatEnd(CombatRoom room)
    {
        ResetCombatDrawBonus();
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
        DynamicVars["DrawBonus"].UpgradeValueBy(2m);
    }

    protected override void AfterDowngraded()
    {
        base.AfterDowngraded();
        DynamicVars[IaidoVar.DefaultName].BaseValue += _combatDrawBonus;
    }

    private void ResetCombatDrawBonus()
    {
        if (_combatDrawBonus == 0m)
        {
            return;
        }

        if (IsMutable)
        {
            DynamicVars[IaidoVar.DefaultName].BaseValue -= _combatDrawBonus;
        }

        _combatDrawBonus = 0m;
    }
}
