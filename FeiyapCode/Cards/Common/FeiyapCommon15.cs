using Feiyap.Characters;
using Feiyap.Mechanics;
using Feiyap.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Scaffolding.Content;

namespace Feiyap.Cards.Common;

/// <summary>
/// 纳刀：不存在居合时才能发动，获得 4 / 7 点居合。
/// </summary>
[RegisterCard(typeof(FeiyapCardPool))]
public sealed class FeiyapCommon15 : FeiyapCardTemplate
{

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        FeiyapKeywords.Iaido
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new IaidoVar(4m, ValueProp.Move)
    ];

    protected override bool IsPlayable =>
        !FeiyapIaidoCmd.HasIaido(Owner.Creature);

    protected override bool ShouldGlowGoldInternal => IsPlayable;

    public FeiyapCommon15()
        : base(0, CardType.Skill, CardRarity.Common, TargetType.Self)
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
    }

    protected override void OnUpgrade()
    {
        DynamicVars[IaidoVar.DefaultName].UpgradeValueBy(3m);
    }
}
