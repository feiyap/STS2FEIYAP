using Feiyap.Mechanics;
using Feiyap.Characters;
using Feiyap.Cards.Tarot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Feiyap.Cards.Common;

/// <summary>
/// VIII-正义：造成 13 / 18 点伤害；正位额外伤害，逆位耗能变为 0。
/// </summary>
[RegisterCard(typeof(FeiyapCardPool))]
public sealed class FeiyapCommon8 : FeiyapTarotCardBase
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(13, ValueProp.Move),
        new DamageVar("BonusDamage", 7, ValueProp.Move)
    ];

    public FeiyapCommon8()
        : base(2, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
    {
        RegisterTarotFactory(player => player.RunState.CreateCard<FeiyapCommon8>(player));
    }

    public override bool TryModifyEnergyCostInCombatLate(
        CardModel card,
        decimal originalCost,
        out decimal modifiedCost)
    {
        modifiedCost = originalCost;
        if (card != this || Owner?.Creature == null)
        {
            return false;
        }

        if (!IsReversed)
        {
            return false;
        }

        if (FeiyapTarotCmd.HasDualEffect(Owner))
        {
            modifiedCost = 0m;
            return true;
        }

        if (IsReversedTriggered(Owner))
        {
            modifiedCost = 0m;
            return true;
        }

        modifiedCost = originalCost + 2m;
        return true;
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        EnsureOrientationInitialized();
        ArgumentNullException.ThrowIfNull(cardPlay.Target);

        var applyUprightBonus = false;
        await RunTarotBranches(
            choiceContext,
            () =>
            {
                applyUprightBonus = true;
                return Task.CompletedTask;
            },
            () => Task.CompletedTask);

        if (FeiyapTarotCmd.HasDualEffect(Owner))
        {
            applyUprightBonus = true;
        }

        var damage = DynamicVars.Damage.BaseValue;
        if (applyUprightBonus)
        {
            damage += DynamicVars["BonusDamage"].BaseValue;
        }

        await DamageCmd.Attack(damage)
            .FromCard(this)
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(5m);
        DynamicVars["BonusDamage"].UpgradeValueBy(2m);
    }
}
