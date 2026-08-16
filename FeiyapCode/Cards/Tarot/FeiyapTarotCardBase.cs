using Feiyap.Mechanics;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Saves.Runs;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Scaffolding.Content;

namespace Feiyap.Cards.Tarot;

/// <summary>
/// 塔罗牌基类：随机正/逆位，并注册到塔罗牌池。
/// </summary>
public abstract class FeiyapTarotCardBase(
    int baseCost,
    CardType type,
    CardRarity rarity,
    TargetType target,
    bool showInCardLibrary = true)
    : FeiyapCardTemplate(baseCost, type, rarity, target, showInCardLibrary)
{
    private bool _isReversed;
    private bool _orientationInitialized;
    private bool? _portraitOrientationOverride;

    /// <summary>
    /// 检查预览等场景下强制指定卡图朝向；不影响牌面 <see cref="IsReversed"/> 与战斗逻辑。
    /// </summary>
    public bool? PortraitOrientationOverride
    {
        get => _portraitOrientationOverride;
        set => _portraitOrientationOverride = value;
    }

    /// <summary>
    /// 是否具备可切换查看的独立逆位卡图。
    /// </summary>
    public bool HasInspectableReversedPortrait =>
        FeiyapCardAssets.HasReversedPortrait(GetType().Name);

    /// <summary>
    /// 卡图是否显示逆位：预览覆盖优先；否则跟随当前可触发的正/逆位效果（与金/红光一致）；
    /// 双效、自选或尚未可触发时，回退到牌面 <see cref="IsReversed"/>。
    /// 图鉴等 canonical 实例不可访问 <see cref="CardModel.Owner"/>，固定显示正位图。
    /// </summary>
    public bool DisplaysReversedPortrait
    {
        get
        {
            if (_portraitOrientationOverride.HasValue)
            {
                return _portraitOrientationOverride.Value;
            }

            // canonical 模型（图鉴/池原型）访问 Owner 会抛 CanonicalModelException
            if (!IsMutable)
            {
                return false;
            }

            var owner = Owner;
            if (owner != null
                && !FeiyapTarotCmd.HasDualEffect(owner)
                && !FeiyapTarotCmd.HasFreeChoice(owner))
            {
                if (IsReversedTriggered(owner))
                {
                    return true;
                }

                if (IsUprightTriggered(owner))
                {
                    return false;
                }
            }

            return IsReversed;
        }
    }

    /// <summary>
    /// 正位使用 <c>{TypeName}.png</c>，逆位优先 <c>{TypeName}_Reversed.png</c>。
    /// </summary>
    public override CardAssetProfile AssetProfile =>
        FeiyapCardAssets.For(GetType().Name, DisplaysReversedPortrait);

    public override string? CustomPortraitPath =>
        FeiyapCardAssets.ResolvePortraitPath(GetType().Name, DisplaysReversedPortrait);

    protected override HashSet<CardTag> CanonicalTags => new() { FeiyapCardTags.Tarot };

    protected override bool ShouldGlowGoldInternal =>
        IsMutable
        && (IsUprightTriggered(Owner)
            || FeiyapTarotCmd.HasDualEffect(Owner)
            || FeiyapTarotCmd.HasFreeChoice(Owner));

    protected override bool ShouldGlowRedInternal =>
        IsMutable
        && (IsReversedTriggered(Owner)
            || FeiyapTarotCmd.HasDualEffect(Owner)
            || FeiyapTarotCmd.HasFreeChoice(Owner));

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        FeiyapKeywords.TarotUpright,
        FeiyapKeywords.TarotReversed
    ];

    [SavedProperty]
    public bool IsReversed
    {
        get => _isReversed;
        set
        {
            AssertMutable();
            if (_isReversed == value)
            {
                return;
            }

            _isReversed = value;
            RefreshPortraitVisuals();
            OnOrientationChanged(value);
        }
    }

    /// <summary>正/逆位切换时的钩子（如 XIV-节制 减费）。</summary>
    protected virtual void OnOrientationChanged(bool isReversed)
    {
    }

    [SavedProperty]
    public bool OrientationInitialized
    {
        get => _orientationInitialized;
        set
        {
            AssertMutable();
            _orientationInitialized = value;
        }
    }

    protected FeiyapTarotCardBase(int baseCost, CardType type, TargetType target, bool showInCardLibrary = true)
        : this(baseCost, type, CardRarity.Token, target, showInCardLibrary)
    {
    }

    protected void RegisterTarotFactory(FeiyapTarotCardFactory factory) =>
        FeiyapTarotRegistry.Register(factory);

    /// <summary>正位效果是否满足触发条件（上一张打出的是技能牌）。</summary>
    public static bool IsUprightEffectTriggeredFor(Player? player) =>
        player != null
        && FeiyapCombatTracker.Get(player).LastPlayedType == CardType.Skill;

    /// <summary>逆位效果是否满足触发条件（上一张打出的是攻击牌）。</summary>
    public static bool IsReversedEffectTriggeredFor(Player? player) =>
        player != null
        && FeiyapCombatTracker.Get(player).LastPlayedType == CardType.Attack;

    /// <summary>正位效果是否满足触发条件（上一张打出的是技能牌）。</summary>
    protected bool IsUprightTriggered(Player? player) =>
        IsUprightEffectTriggeredFor(player);

    /// <summary>逆位效果是否满足触发条件（上一张打出的是攻击牌）。</summary>
    protected bool IsReversedTriggered(Player? player) =>
        IsReversedEffectTriggeredFor(player);

    /// <summary>当前朝向下，对应塔罗效果是否可触发。</summary>
    protected bool IsTarotEffectTriggered(Player? player)
    {
        if (FeiyapTarotCmd.HasDualEffect(player) || FeiyapTarotCmd.HasFreeChoice(player))
        {
            return true;
        }

        return IsUprightTriggered(player) || IsReversedTriggered(player);
    }

    /// <summary>按 XXI-世界 等效果执行塔罗正逆位分支。</summary>
    protected async Task RunTarotBranches(
        PlayerChoiceContext choiceContext,
        Func<Task> uprightEffect,
        Func<Task> reversedEffect)
    {
        var player = Owner;
        if (player == null)
        {
            return;
        }

        if (FeiyapTarotCmd.HasDualEffect(player))
        {
            await uprightEffect();
            await reversedEffect();

            var tracker = FeiyapCombatTracker.Get(player);
            if (tracker.DualTarotPlaysRemaining > 0)
            {
                tracker.DualTarotPlaysRemaining--;
            }

            return;
        }

        if (FeiyapTarotCmd.HasFreeChoice(player))
        {
            var upright = await FeiyapTarotCmd.ChooseUpright(choiceContext, player, this);
            if (upright)
            {
                await uprightEffect();
            }
            else
            {
                await reversedEffect();
            }

            return;
        }

        if (IsUprightTriggered(player))
        {
            await uprightEffect();
        }
        else if (IsReversedTriggered(player))
        {
            await reversedEffect();
        }
        else
        {
            // 上一张既非攻击也非技能时无金/红光；回退到牌面朝向（与卡图显示一致）
            if (IsReversed)
            {
                await reversedEffect();
            }
            else
            {
                await uprightEffect();
            }
        }
    }

    public void RollOrientation(Rng rng)
    {
        IsReversed = rng.NextBool();
    }

    public override Task AfterCardEnteredCombat(CardModel card)
    {
        if (ReferenceEquals(card, this))
        {
            EnsureOrientationInitialized();
        }

        return Task.CompletedTask;
    }

    public override Task AfterCardGeneratedForCombat(CardModel card, Player? creator)
    {
        if (ReferenceEquals(card, this))
        {
            EnsureOrientationInitialized();
        }

        return Task.CompletedTask;
    }

    public override Task BeforeSideTurnStart(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        EnsureOrientationInitialized();
        return Task.CompletedTask;
    }

    protected void EnsureOrientationInitialized()
    {
        if (OrientationInitialized || !IsMutable || Owner == null)
        {
            return;
        }

        RollOrientation(Owner.RunState.Rng.Niche);
        OrientationInitialized = true;
        RefreshPortraitVisuals();
    }

    /// <summary>朝向变更后刷新桌上已有卡面节点的卡图。</summary>
    internal void RefreshPortraitVisuals()
    {
        var node = NCard.FindOnTable(this);
        if (node != null)
        {
            ApplyPortraitToNode(node);
        }
    }

    /// <summary>刷新玩家手牌中所有塔罗牌的卡图。</summary>
    public static void RefreshHandPortraits(Player player)
    {
        var hand = player.PlayerCombatState?.Hand.Cards;
        if (hand == null)
        {
            return;
        }

        foreach (var card in hand)
        {
            if (card is FeiyapTarotCardBase tarot)
            {
                tarot.RefreshPortraitVisuals();
            }
        }
    }

    /// <summary>将当前朝向对应的卡图应用到卡面节点。</summary>
    internal void ApplyPortraitToNode(NCard node)
    {
        if (node.Model != this || !GodotObject.IsInstanceValid(node))
        {
            return;
        }

        var portraitNodeName = Rarity == CardRarity.Ancient ? "%AncientPortrait" : "%Portrait";
        var portraitRect = node.GetNodeOrNull<TextureRect>(portraitNodeName);
        if (portraitRect == null)
        {
            node.Call(NCard.MethodName.Reload);
            return;
        }

        var texture = Portrait;
        if (portraitRect.Texture != texture)
        {
            portraitRect.Texture = texture;
        }
    }
}
