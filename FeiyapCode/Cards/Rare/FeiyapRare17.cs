using Feiyap.Characters;
using Feiyap.Cards.Tarot;
using Feiyap.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Feiyap.Cards.Rare;

/// <summary>
/// XIV-节制：切换正逆位减费；正位击晕，逆位额外回合。
/// </summary>
[RegisterCard(typeof(FeiyapCardPool))]
public sealed class FeiyapRare17 : FeiyapTarotCardBase
{
    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromPower<FeiyapExtraTurnPower>()
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Retain,
        CardKeyword.Exhaust,
        ..base.CanonicalKeywords
    ];

    public FeiyapRare17()
        : base(18, CardType.Skill, CardRarity.Rare, TargetType.AnyEnemy)
    {
        RegisterTarotFactory(player => player.RunState.CreateCard<FeiyapRare17>(player));
    }

    protected override void OnOrientationChanged(bool isReversed)
    {
        // 仅在朝向已初始化后的切换减费，避免首次随机朝向误触发。
        if (OrientationInitialized && IsMutable && Pile?.Type is PileType.Hand or PileType.Play)
        {
            EnergyCost.AddUntilPlayed(-1);
        }
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        EnsureOrientationInitialized();
        ArgumentNullException.ThrowIfNull(cardPlay.Target);

        await RunTarotBranches(
            choiceContext,
            async () => await CreatureCmd.Stun(cardPlay.Target),
            async () =>
            {
                await PowerCmd.Apply(
                    choiceContext,
                    ModelDb.Power<FeiyapExtraTurnPower>().ToMutable(),
                    Owner.Creature,
                    1m,
                    Owner.Creature,
                    this);
            });
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-3);
    }
}
