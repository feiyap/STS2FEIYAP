using Feiyap.Characters;
using Feiyap.Cards.Tarot;
using Feiyap.Mechanics;
using Feiyap.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Feiyap.Cards.Uncommon;

/// <summary>
/// XI-力量：造成 10 / 14 点伤害；正位获得残心，逆位使目标本回合减少力量。
/// </summary>
[RegisterCard(typeof(FeiyapCardPool))]
public sealed class FeiyapUncommon11 : FeiyapTarotCardBase
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(10, ValueProp.Move),
        new PowerVar<FeiyapZanxinPower>(5m),
        new PowerVar<StrengthPower>(5m)
    ];

    public FeiyapUncommon11()
        : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
        RegisterTarotFactory(player => player.RunState.CreateCard<FeiyapUncommon11>(player));
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
                await FeiyapZanxinCmd.Gain(
                    choiceContext,
                    Owner.Creature,
                    DynamicVars["FeiyapZanxinPower"].BaseValue,
                    this);
            },
            async () =>
            {
                await PowerCmd.Apply<FeiyapStrengthDownPower>(
                    choiceContext,
                    cardPlay.Target,
                    DynamicVars["StrengthPower"].BaseValue,
                    Owner.Creature,
                    this);
            });
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(4m);
        DynamicVars["FeiyapZanxinPower"].UpgradeValueBy(2m);
        DynamicVars["StrengthPower"].UpgradeValueBy(2m);
    }
}
