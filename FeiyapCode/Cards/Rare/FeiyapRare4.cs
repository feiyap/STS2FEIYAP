using Feiyap.Characters;
using Feiyap.Mechanics;
using Feiyap.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Feiyap.Cards.Rare;

/// <summary>
/// 枯山水：移除目标防御并禁止其再获得特定防御效果，然后造成伤害。
/// </summary>
[RegisterCard(typeof(FeiyapCardPool))]
public sealed class FeiyapRare4 : FeiyapCardTemplate
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromPower<FeiyapKaresansuiPower>()
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(10, ValueProp.Move)
    ];

    public FeiyapRare4()
        : base(1, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);

        if (cardPlay.Target.Block > 0m)
        {
            await CreatureCmd.LoseBlock(
                choiceContext,
                cardPlay.Target,
                cardPlay.Target.Block,
                Owner.Creature);
        }

        await FeiyapKaresansuiPower.StripForbiddenPowers(cardPlay.Target);

        await PowerCmd.Apply(
            choiceContext,
            ModelDb.Power<FeiyapKaresansuiPower>().ToMutable(),
            cardPlay.Target,
            1m,
            Owner.Creature,
            this);

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, cardPlay)
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(5m);
    }
}
