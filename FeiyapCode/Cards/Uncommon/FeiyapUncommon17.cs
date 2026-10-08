using Feiyap.Characters;
using Feiyap.Mechanics;
using Feiyap.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Scaffolding.Content;

namespace Feiyap.Cards.Uncommon;

/// <summary>
/// 孤燕之瞥：给予 3 / 5 层破绽，获得 3 / 5 层居合强化。
/// </summary>
[RegisterCard(typeof(FeiyapCardPool))]
public sealed class FeiyapUncommon17 : FeiyapCardTemplate
{
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        FeiyapKeywords.IaidoEnhance
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromPower<FeiyapPozhanPower>(),
        HoverTipFactory.FromPower<FeiyapIaidoEnhancePower>()
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<FeiyapPozhanPower>(3m),
        new PowerVar<FeiyapIaidoEnhancePower>(3m)
    ];

    public FeiyapUncommon17()
        : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);

        await PowerCmd.Apply<FeiyapPozhanPower>(
            choiceContext,
            cardPlay.Target,
            DynamicVars["FeiyapPozhanPower"].BaseValue,
            Owner.Creature,
            this);

        await PowerCmd.Apply<FeiyapIaidoEnhancePower>(
            choiceContext,
            Owner.Creature,
            DynamicVars["FeiyapIaidoEnhancePower"].BaseValue,
            Owner.Creature,
            this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["FeiyapPozhanPower"].UpgradeValueBy(2m);
        DynamicVars["FeiyapIaidoEnhancePower"].UpgradeValueBy(2m);
    }
}
