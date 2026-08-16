using Feiyap.Characters;
using Feiyap.Mechanics;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Feiyap.Cards.Rare;

/// <summary>
/// 神座屠：完美居合后可打出；对所有敌人造成多段伤害。
/// </summary>
[RegisterCard(typeof(FeiyapCardPool))]
public sealed class FeiyapRare5 : FeiyapCardTemplate
{
    private bool _witnessedPerfectIaido;

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
        [HoverTipFactory.FromKeyword(FeiyapKeywords.PerfectIaido)];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(7, ValueProp.Move),
        new RepeatVar(3)
    ];

    [SavedProperty]
    public bool WitnessedPerfectIaido
    {
        get => _witnessedPerfectIaido;
        set
        {
            AssertMutable();
            _witnessedPerfectIaido = value;
        }
    }

    protected override bool IsPlayable =>
        Pile?.Type != PileType.Hand
        || WitnessedPerfectIaido
        || (Owner != null && FeiyapCombatTracker.Get(Owner).ShinzatoUnlockedThisTurn);

    protected override bool ShouldGlowGoldInternal =>
        Pile?.Type == PileType.Hand
        && (WitnessedPerfectIaido
            || (Owner != null && FeiyapCombatTracker.Get(Owner).ShinzatoUnlockedThisTurn));

    public FeiyapRare5()
        : base(0, CardType.Attack, CardRarity.Rare, TargetType.AllEnemies)
    {
    }

    public void MarkPerfectIaidoWitnessed() => WitnessedPerfectIaido = true;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .WithHitCount(DynamicVars.Repeat.IntValue)
            .FromCard(this, cardPlay)
            .TargetingAllOpponents(CombatState!)
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Repeat.UpgradeValueBy(2m);
    }
}
