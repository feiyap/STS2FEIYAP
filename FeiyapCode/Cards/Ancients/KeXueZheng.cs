using System.Threading.Tasks;
using Feiyap.Characters;
using Feiyap.Mechanics;
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
/// 握牌时每受到一次伤害积攒段数；传统格挡与居合全额抵消均计入。
/// </summary>
[RegisterCard(typeof(FeiyapCardPool))]
public sealed class KeXueZheng : FeiyapCardTemplate
{
    private int _bonusHitCount;
    private bool _autoPlayPending;

    /// <summary>
    /// 本段伤害在减伤/Cap（含居合）之前已为正；用于 TotalDamage==0 的居合全挡仍计数。
    /// </summary>
    private bool _pendingHitToCount;

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Retain
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(5, ValueProp.Move),
        new RepeatVar(3),
        new IntVar("ExtraHits", 0)
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

    public override decimal ModifyDamageAdditive(
        Creature? target,
        decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource,
        CardPlay? cardPlay)
    {
        // 居合在 Cap 阶段把伤害压到 0，须在加算阶段先记下「受到过一次伤害」。
        if (target == Owner.Creature
            && amount > 0m
            && Pile?.Type == PileType.Hand
            && !FeiyapIaidoIntentPreviewScope.IsActive)
        {
            _pendingHitToCount = true;
        }

        return 0m;
    }

    public override async Task AfterDamageReceived(
        PlayerChoiceContext choiceContext,
        Creature target,
        DamageResult result,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        // 居合反击在 BeforeDamageReceived 中嵌套对敌人造成伤害；
        // 若在此处对「任意目标」清标记，会在自身 AfterDamageReceived 前被反击清掉，导致居合全挡不计次。
        if (target != Owner.Creature)
        {
            return;
        }

        var pendingHit = _pendingHitToCount;
        _pendingHitToCount = false;

        if (Pile?.Type != PileType.Hand)
        {
            return;
        }

        // TotalDamage>0：掉血或传统格挡；pendingHit：含居合全额抵消（此时 TotalDamage 为 0）。
        if (result.TotalDamage <= 0 && !pendingHit)
        {
            return;
        }

        BonusHitCount++;
        FeiyapQuestCardVisuals.RefreshCardVisuals(this);

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
            .FromCard(this, cardPlay)
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);

        BonusHitCount = 0;
    }

    protected override void OnUpgrade()
    {
        // 强化效果由握牌时的濒死自动释放逻辑承担。
    }
}
