using Feiyap.Characters;
using Feiyap.Mechanics;
using Feiyap.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Feiyap.Cards.Uncommon;

/// <summary>
/// 担刀势：移除其他架势；每回合首次命中敌人时给予破绽。
/// </summary>
[RegisterCard(typeof(FeiyapCardPool))]
public sealed class FeiyapUncommon29 : FeiyapCardTemplate
{
    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromPower<FeiyapTandaoStancePower>(),
        HoverTipFactory.FromPower<FeiyapPozhanPower>()
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<FeiyapPozhanPower>(1m)
    ];

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
            DynamicVars["FeiyapPozhanPower"].BaseValue,
            this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["FeiyapPozhanPower"].UpgradeValueBy(1m);
    }
}
