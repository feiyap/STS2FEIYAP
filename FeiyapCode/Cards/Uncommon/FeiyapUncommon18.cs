using Feiyap.Characters;
using Feiyap.Mechanics;
using MegaCrit.Sts2.Core.Commands;using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Saves.Runs;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Feiyap.Cards.Uncommon;

/// <summary>
/// 叶隐：保留；完美居合后可打出；获得耗能并抽牌。消耗。
/// </summary>
[RegisterCard(typeof(FeiyapCardPool))]
public sealed class FeiyapUncommon18 : FeiyapCardTemplate
{
    private bool _witnessedPerfectIaido;

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Retain,
        CardKeyword.Exhaust
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        EnergyHoverTip,
        HoverTipFactory.FromKeyword(FeiyapKeywords.PerfectIaido)
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new EnergyVar(2),
        new CardsVar(2)
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

    public FeiyapUncommon18()
        : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    public void MarkPerfectIaidoWitnessed() => WitnessedPerfectIaido = true;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PlayerCmd.GainEnergy(DynamicVars.Energy.BaseValue, Owner);
        await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.IntValue, Owner);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Energy.UpgradeValueBy(1m);
        DynamicVars.Cards.UpgradeValueBy(1m);
    }
}
