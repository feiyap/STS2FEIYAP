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
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Scaffolding.Content;

namespace Feiyap.Cards.Ancients;

/// <summary>
/// 先古卡：无声星陨。
/// </summary>
[RegisterCard(typeof(FeiyapCardPool))]
public sealed class WuShengXingYun : FeiyapCardTemplate
{
    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromPower<FeiyapHeatDeathPower>(),
        HoverTipFactory.FromCard<XingTianYunZhui>()
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<FeiyapHeatDeathPower>(2m)
    ];

    public WuShengXingYun()
        : base(1, CardType.Skill, CardRarity.Ancient, TargetType.Self, showInCardLibrary: true)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply(
            choiceContext,
            ModelDb.Power<FeiyapHeatDeathPower>().ToMutable(),
            Owner.Creature,
            DynamicVars["FeiyapHeatDeathPower"].BaseValue,
            Owner.Creature,
            this);

        var meteor = Owner.RunState.CreateCard<XingTianYunZhui>(Owner);
        if (IsUpgraded)
        {
            CardCmd.Upgrade(meteor);
        }

        await CardPileCmd.Add(meteor, PileType.Hand);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["FeiyapHeatDeathPower"].UpgradeValueBy(2m);
    }
}
