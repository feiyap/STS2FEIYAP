using System.Linq;
using Feiyap.Characters;
using Feiyap.Cards.Tarot;
using Feiyap.Mechanics;
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

namespace Feiyap.Cards.Rare;

/// <summary>
/// VII-战车：正位击晕攻击意图敌人并获 intangible；逆位随机多段伤害。
/// </summary>
[RegisterCard(typeof(FeiyapCardPool))]
public sealed class FeiyapRare6 : FeiyapTarotCardBase
{
    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromPower<IntangiblePower>()
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<IntangiblePower>(1m),
        new DamageVar(15, ValueProp.Move),
        new RepeatVar(3)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Exhaust,
        ..base.CanonicalKeywords
    ];

    public FeiyapRare6()
        : base(3, CardType.Attack, CardRarity.Rare, TargetType.AllEnemies)
    {
        RegisterTarotFactory(player => player.RunState.CreateCard<FeiyapRare6>(player));
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        EnsureOrientationInitialized();

        await RunTarotBranches(
            choiceContext,
            () => PlayUpright(choiceContext),
            () => PlayReversed(choiceContext, cardPlay));
    }

    protected override void OnUpgrade()
    {
        DynamicVars["IntangiblePower"].UpgradeValueBy(-1m);
        DynamicVars.Damage.UpgradeValueBy(6m);
    }

    private async Task PlayUpright(PlayerChoiceContext choiceContext)
    {
        if (CombatState == null)
        {
            return;
        }

        var intangible = DynamicVars["IntangiblePower"].BaseValue;
        foreach (var enemy in CombatState.HittableEnemies)
        {
            if (!HasAttackIntent(enemy))
            {
                continue;
            }

            await CreatureCmd.Stun(enemy);
            if (intangible > 0m)
            {
                await PowerCmd.Apply<IntangiblePower>(
                    choiceContext,
                    enemy,
                    intangible,
                    Owner.Creature,
                    this);
            }
        }
    }

    private async Task PlayReversed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (CombatState == null)
        {
            return;
        }

        for (var i = 0; i < DynamicVars.Repeat.IntValue; i++)
        {
            var target = Owner.RunState.Rng.CombatTargets
                .NextItem(CombatState.HittableEnemies);
            if (target == null)
            {
                break;
            }

            await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
                .FromCard(this, cardPlay)
                .Targeting(target)
                .Execute(choiceContext);
        }
    }

    private static bool HasAttackIntent(MegaCrit.Sts2.Core.Entities.Creatures.Creature enemy) =>
        enemy.Monster?.NextMove?.Intents.Any(i => i is AttackIntent) == true;
}
