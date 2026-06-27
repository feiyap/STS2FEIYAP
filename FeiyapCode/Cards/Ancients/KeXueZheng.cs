using System.Threading.Tasks;
using Feiyap.Characters;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Feiyap.Cards.Ancients;

/// <summary>
/// 先古卡：渴血症。
/// </summary>
[RegisterCard(typeof(FeiyapCardPool))]
public sealed class KeXueZheng : FeiyapCardTemplate
{
    private int _bonusHitCount;
    private bool _autoPlayPending;

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Retain
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(5, ValueProp.Move),
        new RepeatVar(3),
        new DynamicVar("ExtraHits", 0m)
    ];

    [SavedProperty]
    public int BonusHitCount
    {
        get => _bonusHitCount;
        set
        {
            AssertMutable();
            _bonusHitCount = Math.Max(0, value);
            DynamicVars["ExtraHits"].BaseValue = _bonusHitCount;
        }
    }

    public KeXueZheng()
        : base(3, CardType.Attack, CardRarity.Ancient, TargetType.AnyEnemy, showInCardLibrary: true)
    {
    }

    public override int ModifyAttackHitCount(AttackCommand attack, int hitCount)
    {
        if (attack.ModelSource != this)
        {
            return hitCount;
        }

        return hitCount + BonusHitCount;
    }

    public override async Task AfterDamageReceived(
        PlayerChoiceContext choiceContext,
        Creature target,
        DamageResult result,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        if (target != Owner.Creature || Pile?.Type != PileType.Hand || result.TotalDamage <= 0)
        {
            return;
        }

        BonusHitCount++;

        if (!IsUpgraded || Owner.Creature.CurrentHp >= 1m || _autoPlayPending)
        {
            return;
        }

        _autoPlayPending = true;
        try
        {
            await CreatureCmd.SetCurrentHp(Owner.Creature, 1m);
            await CardCmd.AutoPlay(choiceContext, this, null);
            if (Pile != null)
            {
                await CardCmd.Exhaust(choiceContext, this);
            }
        }
        finally
        {
            _autoPlayPending = false;
        }
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .WithHitCount(DynamicVars.Repeat.IntValue)
            .FromCard(this)
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);

        BonusHitCount = 0;
    }

    protected override void OnUpgrade()
    {
        // 强化效果由握牌时的濒死自动释放逻辑承担。
    }
}
