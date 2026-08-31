using Feiyap.Characters;
using Feiyap.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Feiyap.Cards.Rare;

/// <summary>
/// 无我梦中：本回合对目标伤害提升，其他敌人对你的伤害降低。
/// </summary>
[RegisterCard(typeof(FeiyapCardPool))]
public sealed class FeiyapRare15 : FeiyapCardTemplate
{
    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromPower<FeiyapMugaMuchuPower>()
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("Bonus", 50m)
    ];

    public FeiyapRare15()
        : base(1, CardType.Skill, CardRarity.Rare, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);

        var power = (FeiyapMugaMuchuPower)ModelDb.Power<FeiyapMugaMuchuPower>().ToMutable();
        power.MarkedTarget = cardPlay.Target;
        await PowerCmd.Apply(
            choiceContext,
            power,
            Owner.Creature,
            DynamicVars["Bonus"].BaseValue,
            Owner.Creature,
            this);

        var applied = Owner.Creature.GetPower<FeiyapMugaMuchuPower>();
        if (applied != null)
        {
            applied.MarkedTarget = cardPlay.Target;
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Bonus"].UpgradeValueBy(50m);
    }
}
