using Feiyap.Characters;
using Feiyap.Cards.Tarot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Feiyap.Cards.Rare;

/// <summary>
/// V-教皇：正位获得再生，逆位获得覆甲。
/// </summary>
[RegisterCard(typeof(FeiyapCardPool))]
public sealed class FeiyapRare24 : FeiyapTarotCardBase
{
    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromPower<RegenPower>(),
        HoverTipFactory.FromPower<PlatingPower>()
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<RegenPower>(4m),
        new PowerVar<PlatingPower>(4m)
    ];

    public FeiyapRare24()
        : base(1, CardType.Power, CardRarity.Rare, TargetType.Self)
    {
        RegisterTarotFactory(player => player.RunState.CreateCard<FeiyapRare24>(player));
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        EnsureOrientationInitialized();
        await CreatureCmd.TriggerAnim(Owner.Creature, "PowerUp", Owner.Character.PowerUpAnimDelay);

        await RunTarotBranches(
            choiceContext,
            async () =>
            {
                await PowerCmd.Apply<RegenPower>(
                    choiceContext,
                    Owner.Creature,
                    DynamicVars["RegenPower"].BaseValue,
                    Owner.Creature,
                    this);
            },
            async () =>
            {
                await PowerCmd.Apply<PlatingPower>(
                    choiceContext,
                    Owner.Creature,
                    DynamicVars["PlatingPower"].BaseValue,
                    Owner.Creature,
                    this);
            });
    }

    protected override void OnUpgrade()
    {
        DynamicVars["RegenPower"].UpgradeValueBy(2m);
        DynamicVars["PlatingPower"].UpgradeValueBy(2m);
    }
}
