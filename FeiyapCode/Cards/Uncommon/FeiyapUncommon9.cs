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
/// XI-力量：对所有敌人造成 8 / 11 点伤害；正位获得 3 / 5 点残心，逆位使所有敌人本回合减少 3 / 5 点力量。
/// </summary>
[RegisterCard(typeof(FeiyapCardPool))]
public sealed class FeiyapUncommon9 : FeiyapTarotCardBase
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(8, ValueProp.Move),
        new PowerVar<FeiyapZanxinPower>(3m),
        new PowerVar<StrengthPower>(3m)
    ];

    public FeiyapUncommon9()
        : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AllEnemies)
    {
        RegisterTarotFactory(player => player.RunState.CreateCard<FeiyapUncommon9>(player));
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        EnsureOrientationInitialized();

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, cardPlay)
            .TargetingAllOpponents(CombatState!)
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
                if (CombatState == null)
                {
                    return;
                }

                foreach (var enemy in CombatState.HittableEnemies)
                {
                    await PowerCmd.Apply<FeiyapStrengthDownPower>(
                        choiceContext,
                        enemy,
                        DynamicVars["StrengthPower"].BaseValue,
                        Owner.Creature,
                        this);
                }
            });
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3m);
        DynamicVars["FeiyapZanxinPower"].UpgradeValueBy(2m);
        DynamicVars["StrengthPower"].UpgradeValueBy(2m);
    }
}
