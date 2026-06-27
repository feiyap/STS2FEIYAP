using Feiyap.Characters;
using Feiyap.Cards.Tarot;
using Feiyap.Mechanics;
using Feiyap.Powers;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Feiyap.Cards.Common;

/// <summary>
/// X-命运之轮：塔罗技能牌，正位获得活力与残心，逆位对所有敌人施加易伤与虚弱。
/// </summary>
[RegisterCard(typeof(FeiyapCardPool))]
public sealed class FeiyapCommon19 : FeiyapTarotCardBase
{

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<VigorPower>(2m),
        new PowerVar<FeiyapZanxinPower>(2m),
        new PowerVar<VulnerablePower>(1m),
        new PowerVar<WeakPower>(1m)
    ];

    protected override bool ShouldGlowGoldInternal => IsTarotEffectTriggered(Owner);

    public FeiyapCommon19()
        : base(1, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
        RegisterTarotFactory(player => player.RunState.CreateCard<FeiyapCommon19>(player));
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        EnsureOrientationInitialized();

        if (IsUprightTriggered(Owner))
        {
            await PowerCmd.Apply<VigorPower>(
                choiceContext,
                Owner.Creature,
                DynamicVars["VigorPower"].BaseValue,
                Owner.Creature,
                this);

            await FeiyapZanxinCmd.Gain(
                choiceContext,
                Owner.Creature,
                DynamicVars["FeiyapZanxinPower"].BaseValue,
                this);
            return;
        }

        if (!IsReversedTriggered(Owner))
        {
            return;
        }

        if (Owner.Creature.CombatState is not CombatState state)
        {
            return;
        }

        foreach (var enemy in state.HittableEnemies)
        {
            await PowerCmd.Apply<VulnerablePower>(
                choiceContext,
                enemy,
                DynamicVars["VulnerablePower"].BaseValue,
                Owner.Creature,
                this);

            await PowerCmd.Apply<WeakPower>(
                choiceContext,
                enemy,
                DynamicVars["WeakPower"].BaseValue,
                Owner.Creature,
                this);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["VigorPower"].UpgradeValueBy(1m);
        DynamicVars["FeiyapZanxinPower"].UpgradeValueBy(1m);
        DynamicVars["VulnerablePower"].UpgradeValueBy(1m);
        DynamicVars["WeakPower"].UpgradeValueBy(1m);
    }
}
