using System.Collections.Generic;
using System.Threading.Tasks;
using Feiyap.Characters;
using Feiyap.Mechanics;
using Feiyap.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Feiyap.Cards.Ancients;

/// <summary>
/// 先古卡：斋时雨（原版先古遗物「古老牙齿」由居合术超越获得）。
/// </summary>
[RegisterCard(typeof(FeiyapCardPool))]
public sealed class ZhaiShiYu : FeiyapCardTemplate
{
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        FeiyapKeywords.Iaido
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<FeiyapIaidoPower>(12m)
    ];

    public ZhaiShiYu()
        : base(1, CardType.Skill, CardRarity.Ancient, TargetType.Self, showInCardLibrary: true)
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

        await PowerCmd.Apply(
            choiceContext,
            ModelDb.Power<FeiyapIaidoRainPower>().ToMutable(),
            Owner.Creature,
            1m,
            Owner.Creature,
            this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["FeiyapIaidoPower"].UpgradeValueBy(9m);
    }
}
