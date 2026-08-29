using System.Linq;
using Feiyap.Characters;
using Feiyap.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Feiyap.Cards.Uncommon;

/// <summary>
/// 分铜锁：造成 14 / 18 点伤害；目标意图为攻击则给予虚弱，否则给予破绽。
/// </summary>
[RegisterCard(typeof(FeiyapCardPool))]
public sealed class FeiyapUncommon6 : FeiyapCardTemplate
{
    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromPower<WeakPower>(),
        HoverTipFactory.FromPower<FeiyapPozhanPower>()
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(14, ValueProp.Move),
        new PowerVar<WeakPower>(2m),
        new PowerVar<FeiyapPozhanPower>(2m)
    ];

    public FeiyapUncommon6()
        : base(2, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, cardPlay)
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);

        if (cardPlay.Target is not { IsAlive: true })
        {
            return;
        }

        if (HasAttackIntent(cardPlay.Target))
        {
            await PowerCmd.Apply<WeakPower>(
                choiceContext,
                cardPlay.Target,
                DynamicVars["WeakPower"].BaseValue,
                Owner.Creature,
                this);
        }
        else
        {
            await PowerCmd.Apply<FeiyapPozhanPower>(
                choiceContext,
                cardPlay.Target,
                DynamicVars["FeiyapPozhanPower"].BaseValue,
                Owner.Creature,
                this);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(4m);
        DynamicVars["WeakPower"].UpgradeValueBy(1m);
        DynamicVars["FeiyapPozhanPower"].UpgradeValueBy(1m);
    }

    private static bool HasAttackIntent(MegaCrit.Sts2.Core.Entities.Creatures.Creature enemy) =>
        enemy.Monster?.NextMove?.Intents.Any(i => i is AttackIntent) == true;
}
