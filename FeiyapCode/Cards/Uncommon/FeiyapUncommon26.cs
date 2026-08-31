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
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Feiyap.Cards.Uncommon;

/// <summary>
/// VI-恋人：给予破绽；正位扩散破绽，逆位按其层数获得力量。
/// </summary>
[RegisterCard(typeof(FeiyapCardPool))]
public sealed class FeiyapUncommon26 : FeiyapTarotCardBase
{
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        FeiyapKeywords.TarotUpright,
        FeiyapKeywords.TarotReversed,
        CardKeyword.Exhaust
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromPower<FeiyapPozhanPower>(),
        HoverTipFactory.FromPower<StrengthPower>()
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<FeiyapPozhanPower>(1m)
    ];

    public FeiyapUncommon26()
        : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
        RegisterTarotFactory(player => player.RunState.CreateCard<FeiyapUncommon26>(player));
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        EnsureOrientationInitialized();
        ArgumentNullException.ThrowIfNull(cardPlay.Target);

        await PowerCmd.Apply<FeiyapPozhanPower>(
            choiceContext,
            cardPlay.Target,
            DynamicVars["FeiyapPozhanPower"].BaseValue,
            Owner.Creature,
            this);

        var stacks = cardPlay.Target.GetPowerAmount<FeiyapPozhanPower>();
        await RunTarotBranches(
            choiceContext,
            async () =>
            {
                if (stacks <= 0 || CombatState == null)
                {
                    return;
                }

                foreach (var enemy in CombatState.HittableEnemies)
                {
                    if (enemy == cardPlay.Target)
                    {
                        continue;
                    }

                    await PowerCmd.Apply<FeiyapPozhanPower>(
                        choiceContext,
                        enemy,
                        stacks,
                        Owner.Creature,
                        this);
                }
            },
            async () =>
            {
                if (stacks <= 0)
                {
                    return;
                }

                await PowerCmd.Apply<StrengthPower>(
                    choiceContext,
                    Owner.Creature,
                    stacks,
                    Owner.Creature,
                    this);
            });
    }

    protected override void OnUpgrade()
    {
        DynamicVars["FeiyapPozhanPower"].UpgradeValueBy(1m);
    }
}
