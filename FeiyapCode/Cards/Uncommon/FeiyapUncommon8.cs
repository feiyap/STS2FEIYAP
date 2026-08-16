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

namespace Feiyap.Cards.Uncommon;

/// <summary>
/// 秘剑：保留；触发完美居合后直到下次打出前耗能变为 0；造成 33 / 44 点伤害。
/// </summary>
[RegisterCard(typeof(FeiyapCardPool))]
public sealed class FeiyapUncommon8 : FeiyapCardTemplate
{
    private bool _perfectIaidoCostReady;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Retain];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromKeyword(FeiyapKeywords.PerfectIaido)
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(33, ValueProp.Move)
    ];

    [SavedProperty]
    public bool PerfectIaidoCostReady
    {
        get => _perfectIaidoCostReady;
        set
        {
            AssertMutable();
            _perfectIaidoCostReady = value;
        }
    }

    protected override bool ShouldGlowGoldInternal => PerfectIaidoCostReady;

    public FeiyapUncommon8()
        : base(3, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
    }

    public void OnPerfectIaidoTriggered()
    {
        PerfectIaidoCostReady = true;
        EnergyCost.SetUntilPlayed(0);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        PerfectIaidoCostReady = false;

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, cardPlay)
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(11m);
    }
}
