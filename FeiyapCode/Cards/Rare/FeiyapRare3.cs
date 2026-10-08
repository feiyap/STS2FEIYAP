using Feiyap.Characters;
using Feiyap.Mechanics;
using Feiyap.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Feiyap.Cards.Rare;

/// <summary>
/// 天人五衰：多段攻击并施加虚弱、易伤、破绽，使目标失去力量与敏捷；本回合每打出过一张攻击牌，耗能减少 1。
/// </summary>
[RegisterCard(typeof(FeiyapCardPool))]
public sealed class FeiyapRare3 : FeiyapCardTemplate
{
    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromPower<WeakPower>(),
        HoverTipFactory.FromPower<VulnerablePower>(),
        HoverTipFactory.FromPower<FeiyapPozhanPower>(),
        HoverTipFactory.FromPower<StrengthPower>(),
        HoverTipFactory.FromPower<DexterityPower>()
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(3, ValueProp.Move),
        new RepeatVar(5),
        new PowerVar<WeakPower>(2m),
        new PowerVar<VulnerablePower>(2m),
        new PowerVar<FeiyapPozhanPower>(2m),
        new PowerVar<StrengthPower>(2m),
        new PowerVar<DexterityPower>(2m)
    ];

    public FeiyapRare3()
        : base(3, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
    {
    }

    public override bool TryModifyEnergyCostInCombatLate(
        CardModel card,
        decimal originalCost,
        out decimal modifiedCost)
    {
        modifiedCost = originalCost;
        if (card != this || Owner == null)
        {
            return false;
        }

        var reduction = FeiyapCombatTracker.Get(Owner).AttacksPlayedThisTurn;
        if (reduction <= 0)
        {
            return false;
        }

        modifiedCost = Math.Max(0m, originalCost - reduction);
        return true;
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .WithHitCount(DynamicVars.Repeat.IntValue)
            .FromCard(this, cardPlay)
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);

        if (!cardPlay.Target.IsAlive)
        {
            return;
        }

        var stacks = DynamicVars["WeakPower"].BaseValue;
        var opening = DynamicVars["FeiyapPozhanPower"].BaseValue;
        var statLoss = DynamicVars["StrengthPower"].BaseValue;
        await PowerCmd.Apply<WeakPower>(choiceContext, cardPlay.Target, stacks, Owner.Creature, this);
        await PowerCmd.Apply<VulnerablePower>(
            choiceContext,
            cardPlay.Target,
            DynamicVars["VulnerablePower"].BaseValue,
            Owner.Creature,
            this);
        await PowerCmd.Apply<FeiyapPozhanPower>(choiceContext, cardPlay.Target, opening, Owner.Creature, this);
        await PowerCmd.Apply<StrengthPower>(choiceContext, cardPlay.Target, -statLoss, Owner.Creature, this);
        await PowerCmd.Apply<DexterityPower>(
            choiceContext,
            cardPlay.Target,
            -DynamicVars["DexterityPower"].BaseValue,
            Owner.Creature,
            this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["WeakPower"].UpgradeValueBy(1m);
        DynamicVars["VulnerablePower"].UpgradeValueBy(1m);
        DynamicVars["FeiyapPozhanPower"].UpgradeValueBy(1m);
        DynamicVars["StrengthPower"].UpgradeValueBy(1m);
        DynamicVars["DexterityPower"].UpgradeValueBy(1m);
    }
}
