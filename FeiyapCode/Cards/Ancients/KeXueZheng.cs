using Feiyap.Characters;
using Feiyap.Mechanics;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Feiyap.Cards.Ancients;

/// <summary>
/// 先古卡：渴血症。
/// 每受到一次伤害积攒段数（未升级仅手牌）；打出后与战斗结束时重置。
/// 传统格挡与居合全额抵消均计入。
/// </summary>
[RegisterCard(typeof(FeiyapCardPool))]
public sealed class KeXueZheng : FeiyapCardTemplate
{
    private const int BaseHitCount = 3;

    private int _bonusHitCount;

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
        new RepeatVar(BaseHitCount),
        new ComputedDynamicVar("ExtraHits", 0, card =>
            card is KeXueZheng kexue ? kexue.BonusHitCount : 0)
    ];

    [SavedProperty]
    public int BonusHitCount
    {
        get => _bonusHitCount;
        set
        {
            AssertMutable();
            _bonusHitCount = Math.Max(0, value);
            SyncHitVars();
        }
    }

    public KeXueZheng()
        : base(3, CardType.Attack, CardRarity.Ancient, TargetType.AnyEnemy, showInCardLibrary: true)
    {
    }

    /// <summary>
    /// 只在战斗牌堆的克隆上计数，避免牌库本体与战斗牌各计一次。
    /// 未升级仅手牌；升级后任意战斗牌堆。
    /// </summary>
    private bool ShouldCountHits()
    {
        if (FeiyapIaidoIntentPreviewScope.IsActive || Pile?.IsCombatPile != true)
        {
            return false;
        }

        return IsUpgraded || Pile.Type == PileType.Hand;
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
        if (target == Owner.Creature && amount > 0m && ShouldCountHits())
        {
            _pendingHitToCount = true;
        }

        return 0m;
    }

    public override Task AfterDamageReceived(
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
            return Task.CompletedTask;
        }

        var pendingHit = _pendingHitToCount;
        _pendingHitToCount = false;

        if (!ShouldCountHits())
        {
            return Task.CompletedTask;
        }

        // TotalDamage>0：掉血或传统格挡；pendingHit：含居合全额抵消（此时 TotalDamage 为 0）。
        if (result.TotalDamage <= 0 && !pendingHit)
        {
            return Task.CompletedTask;
        }

        BonusHitCount++;
        FeiyapQuestCardVisuals.RefreshCardVisuals(this);
        return Task.CompletedTask;
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

    public override Task AfterCardEnteredCombat(CardModel card)
    {
        if (ReferenceEquals(card, this) && IsMutable)
        {
            SyncHitVars();
        }

        return Task.CompletedTask;
    }

    public override Task BeforeCombatStart()
    {
        ResetBonusHits();
        return Task.CompletedTask;
    }

    public override Task AfterCombatEnd(CombatRoom room)
    {
        ResetBonusHits();
        return Task.CompletedTask;
    }

    private void ResetBonusHits()
    {
        if (!IsMutable)
        {
            return;
        }

        BonusHitCount = 0;
        _pendingHitToCount = false;
    }

    private void SyncHitVars()
    {
        DynamicVars.Repeat.BaseValue = BaseHitCount + _bonusHitCount;
    }
}
