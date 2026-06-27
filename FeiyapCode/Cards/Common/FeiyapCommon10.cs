using Feiyap.Characters;
using Feiyap.Mechanics;
using Feiyap.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Scaffolding.Content;

namespace Feiyap.Cards.Common;

/// <summary>
/// 速纳术：本回合内从卡牌中获得居合时，额外获得 2 / 3 点。
/// </summary>
[RegisterCard(typeof(FeiyapCardPool))]
public sealed class FeiyapCommon10 : FeiyapCardTemplate
{

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        FeiyapKeywords.Iaido
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<FeiyapQuickAbsorptionPower>(2m)
    ];

    public FeiyapCommon10()
        : base(0, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply(
            choiceContext,
            ModelDb.Power<FeiyapQuickAbsorptionPower>().ToMutable(),
            Owner.Creature,
            DynamicVars["FeiyapQuickAbsorptionPower"].BaseValue,
            Owner.Creature,
            this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["FeiyapQuickAbsorptionPower"].UpgradeValueBy(1m);
    }
}
