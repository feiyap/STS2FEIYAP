using Feiyap.Characters;
using Feiyap.Mechanics;
using Feiyap.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Feiyap.Cards.Uncommon;

/// <summary>
/// 担刀势：移除其他架势；攻击牌伤害提升 25%。
/// </summary>
[RegisterCard(typeof(FeiyapCardPool))]
public sealed class FeiyapUncommon29 : FeiyapCardTemplate
{
    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
        [HoverTipFactory.FromPower<FeiyapTandaoStancePower>()];

    public FeiyapUncommon29()
        : base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "PowerUp", Owner.Character.PowerUpAnimDelay);
        await FeiyapStanceCmd.ApplyExclusiveStance<FeiyapTandaoStancePower>(
            choiceContext,
            Owner.Creature,
            1m,
            this);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
