using Feiyap.Cards.Tarot;
using Feiyap.Characters;
using Feiyap.Mechanics;
using Feiyap.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Scaffolding.Content;

namespace Feiyap.Cards.Common;

/// <summary>
/// IV-皇帝：造成 8 / 12 点伤害；正位获得 1 层居合强化，逆位给予 1 层易伤。
/// </summary>
[RegisterCard(typeof(FeiyapCardPool))]
public sealed class FeiyapCommon7 : FeiyapTarotCardBase
{
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        FeiyapKeywords.TarotUpright,
        FeiyapKeywords.TarotReversed,
        FeiyapKeywords.IaidoEnhance
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromPower<FeiyapIaidoEnhancePower>(),
        HoverTipFactory.FromPower<VulnerablePower>()
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(8, ValueProp.Move),
        new PowerVar<FeiyapIaidoEnhancePower>(1m),
        new PowerVar<VulnerablePower>(1m)
    ];

    public FeiyapCommon7()
        : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
    {
        RegisterTarotFactory(player => player.RunState.CreateCard<FeiyapCommon7>(player));
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        EnsureOrientationInitialized();
        ArgumentNullException.ThrowIfNull(cardPlay.Target);

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, cardPlay)
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);

        await RunTarotBranches(
            choiceContext,
            async () =>
            {
                await PowerCmd.Apply<FeiyapIaidoEnhancePower>(
                    choiceContext,
                    Owner.Creature,
                    DynamicVars["FeiyapIaidoEnhancePower"].BaseValue,
                    Owner.Creature,
                    this);
            },
            async () =>
            {
                if (!cardPlay.Target.IsAlive)
                {
                    return;
                }

                await PowerCmd.Apply<VulnerablePower>(
                    choiceContext,
                    cardPlay.Target,
                    DynamicVars["VulnerablePower"].BaseValue,
                    Owner.Creature,
                    this);
            });
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(4m);
    }
}
