using Feiyap.Characters;
using Feiyap.Mechanics;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Feiyap.Cards.Rare;

/// <summary>
/// 枯山水：保留；完美居合后可打出；造成 11 点伤害多次。
/// </summary>
[RegisterCard(typeof(FeiyapCardPool))]
public sealed class FeiyapRare4 : FeiyapCardTemplate
{
    private bool _witnessedPerfectIaido;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Retain];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
        [HoverTipFactory.FromKeyword(FeiyapKeywords.PerfectIaido)];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(11, ValueProp.Move),
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
        Pile?.Type != PileType.Hand || WitnessedPerfectIaido;

    protected override bool ShouldGlowGoldInternal =>
        Pile?.Type == PileType.Hand && WitnessedPerfectIaido;

    public FeiyapRare4()
        : base(0, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
    {
    }

    public void MarkPerfectIaidoWitnessed() => WitnessedPerfectIaido = true;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .WithHitCount(DynamicVars.Repeat.IntValue)
            .FromCard(this, cardPlay)
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Repeat.UpgradeValueBy(2m);
    }
}
