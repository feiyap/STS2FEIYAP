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

namespace Feiyap.Cards.Rare;

/// <summary>
/// 剑圣心：获得居合并使居合反击对所有敌人造成伤害。
/// </summary>
[RegisterCard(typeof(FeiyapCardPool))]
public sealed class FeiyapRare22 : FeiyapCardTemplate
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [FeiyapKeywords.Iaido];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
        [HoverTipFactory.FromPower<FeiyapSwordSaintHeartPower>()];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new IaidoVar(6m, ValueProp.Move)
    ];

    public FeiyapRare22()
        : base(1, CardType.Power, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "PowerUp", Owner.Character.PowerUpAnimDelay);

        await FeiyapIaidoCmd.Gain(
            choiceContext,
            Owner.Creature,
            DynamicVars[IaidoVar.DefaultName].BaseValue,
            ValueProp.Move,
            this,
            cardPlay);

        await PowerCmd.Apply(
            choiceContext,
            ModelDb.Power<FeiyapSwordSaintHeartPower>().ToMutable(),
            Owner.Creature,
            1m,
            Owner.Creature,
            this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars[IaidoVar.DefaultName].UpgradeValueBy(3m);
    }
}
