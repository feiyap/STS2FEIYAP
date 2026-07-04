using Feiyap.Characters;
using Feiyap.Mechanics;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Scaffolding.Content;

namespace Feiyap.Cards.Rare;

/// <summary>
/// 流风遗韵：依据本回合造成的伤害量获得等量居合。
/// </summary>
[RegisterCard(typeof(FeiyapCardPool))]
public sealed class FeiyapRare9 : FeiyapCardTemplate
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [FeiyapKeywords.Iaido];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("RecordedDamage", 0m)
    ];

    public FeiyapRare9()
        : base(2, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }

    public override Task BeforeSideTurnStart(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        SyncRecordedDamage();
        return Task.CompletedTask;
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        SyncRecordedDamage();
        var amount = DynamicVars["RecordedDamage"].BaseValue;
        if (amount <= 0m)
        {
            return;
        }

        await FeiyapIaidoCmd.Gain(
            choiceContext,
            Owner.Creature,
            amount,
            ValueProp.Move,
            this,
            cardPlay);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }

    private void SyncRecordedDamage()
    {
        if (Owner == null)
        {
            return;
        }

        DynamicVars["RecordedDamage"].BaseValue = FeiyapCombatTracker.Get(Owner).TurnDamageDealt;
    }
}
