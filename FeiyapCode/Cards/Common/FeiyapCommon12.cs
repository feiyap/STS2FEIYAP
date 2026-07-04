using Feiyap.Characters;
using Feiyap.Mechanics;
using Feiyap.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Scaffolding.Content;

namespace Feiyap.Cards.Common;

/// <summary>
/// 心传：获得 11 / 14 点格挡，获得 5 / 8 点残心。
/// </summary>
[RegisterCard(typeof(FeiyapCardPool))]
public sealed class FeiyapCommon12 : FeiyapCardTemplate
{
    public override bool GainsBlock => true;


    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        FeiyapKeywords.Zanxin
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(11m, ValueProp.Move),
        new PowerVar<FeiyapZanxinPower>(5m)
    ];

    public FeiyapCommon12()
        : base(2, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
        await FeiyapZanxinCmd.Gain(
            choiceContext,
            Owner.Creature,
            DynamicVars["FeiyapZanxinPower"].BaseValue,
            this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(3m);
        DynamicVars["FeiyapZanxinPower"].UpgradeValueBy(3m);
    }
}
