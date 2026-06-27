using System.Collections.Generic;
using System.Threading.Tasks;
using Feiyap.Characters;
using Feiyap.Mechanics;
using Feiyap.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Scaffolding.Content;

namespace Feiyap.Cards.Ancients;

/// <summary>
/// 先古卡：绯樱狱华落。
/// </summary>
[RegisterCard(typeof(FeiyapCardPool))]
public sealed class FeiYingYuHuaLuo : FeiyapCardTemplate
{
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Exhaust,
        FeiyapKeywords.Iaido
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromPower<FeiyapIaidoPower>(),
        HoverTipFactory.FromPower<FeiyapIaidoSurgePower>()
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<FeiyapIaidoPower>(999m),
        new DynamicVar("IaidoSurgePercent", 500m)
    ];

    public FeiYingYuHuaLuo()
        : base(3, CardType.Skill, CardRarity.Ancient, TargetType.Self, showInCardLibrary: true)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await FeiyapIaidoCmd.Gain(
            choiceContext,
            Owner.Creature,
            DynamicVars["FeiyapIaidoPower"].BaseValue,
            ValueProp.Move,
            this,
            cardPlay);

        var surgeMultiplier = DynamicVars["IaidoSurgePercent"].BaseValue / 100m + 1m;
        await PowerCmd.Apply(
            choiceContext,
            ModelDb.Power<FeiyapIaidoSurgePower>().ToMutable(),
            Owner.Creature,
            surgeMultiplier,
            Owner.Creature,
            this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["IaidoSurgePercent"].UpgradeValueBy(500m);
    }
}
