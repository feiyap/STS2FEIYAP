using Feiyap.Characters;
using Feiyap.Cards.Tarot;
using Feiyap.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Feiyap.Cards.Rare;

/// <summary>
/// XX-审判：获得力量与敏捷；正位回合开始失去力量，逆位回合开始失去敏捷。
/// </summary>
[RegisterCard(typeof(FeiyapCardPool))]
public sealed class FeiyapRare26 : FeiyapTarotCardBase
{
    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromPower<StrengthPower>(),
        HoverTipFactory.FromPower<DexterityPower>(),
        HoverTipFactory.FromPower<FeiyapJudgementUprightPower>(),
        HoverTipFactory.FromPower<FeiyapJudgementReversedPower>()
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<StrengthPower>(5m),
        new PowerVar<DexterityPower>(5m)
    ];

    public FeiyapRare26()
        : base(1, CardType.Power, CardRarity.Rare, TargetType.Self)
    {
        RegisterTarotFactory(player => player.RunState.CreateCard<FeiyapRare26>(player));
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        EnsureOrientationInitialized();
        await CreatureCmd.TriggerAnim(Owner.Creature, "PowerUp", Owner.Character.PowerUpAnimDelay);

        await PowerCmd.Apply<StrengthPower>(
            choiceContext,
            Owner.Creature,
            DynamicVars["StrengthPower"].BaseValue,
            Owner.Creature,
            this);
        await PowerCmd.Apply<DexterityPower>(
            choiceContext,
            Owner.Creature,
            DynamicVars["DexterityPower"].BaseValue,
            Owner.Creature,
            this);

        await RunTarotBranches(
            choiceContext,
            async () =>
            {
                await PowerCmd.Apply(
                    choiceContext,
                    ModelDb.Power<FeiyapJudgementUprightPower>().ToMutable(),
                    Owner.Creature,
                    1m,
                    Owner.Creature,
                    this);
            },
            async () =>
            {
                await PowerCmd.Apply(
                    choiceContext,
                    ModelDb.Power<FeiyapJudgementReversedPower>().ToMutable(),
                    Owner.Creature,
                    1m,
                    Owner.Creature,
                    this);
            });
    }

    protected override void OnUpgrade()
    {
        DynamicVars["StrengthPower"].UpgradeValueBy(3m);
        DynamicVars["DexterityPower"].UpgradeValueBy(3m);
    }
}
