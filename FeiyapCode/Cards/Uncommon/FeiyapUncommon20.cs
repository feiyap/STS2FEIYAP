using Feiyap.Characters;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Saves.Runs;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Feiyap.Cards.Uncommon;

/// <summary>
/// 叶隐：保留；握在手中时，触发过完美居合后才可使用。抽 3 张牌，获得 3 点耗能。消耗。
/// </summary>
[RegisterCard(typeof(FeiyapCardPool))]
public sealed class FeiyapUncommon20 : ModCardTemplate
{
    private bool _witnessedPerfectIaido;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/{GetType().Name}.png");

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Retain,
        CardKeyword.Exhaust
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        EnergyHoverTip
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CardsVar(3),
        new EnergyVar(3)
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

    public FeiyapUncommon20()
        : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    public void MarkPerfectIaidoWitnessed()
    {
        WitnessedPerfectIaido = true;
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.IntValue, Owner);
        await PlayerCmd.GainEnergy(DynamicVars.Energy.BaseValue, Owner);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
