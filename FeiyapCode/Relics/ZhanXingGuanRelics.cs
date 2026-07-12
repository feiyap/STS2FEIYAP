using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Feiyap.Characters;
using Feiyap.Enchantments;
using Feiyap.Mechanics;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Enchantments;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Feiyap.Relics;

/// <summary>
/// 斩星官先古遗物共用基类。
/// </summary>
public abstract class ZhanXingGuanRelicBase : ModRelicTemplate
{
    public override RelicRarity Rarity => RelicRarity.Ancient;

    public override RelicAssetProfile AssetProfile =>
        FeiyapRelicAssets.For(GetType().Name);
}

/// <summary>
/// 勾陈一：从牌组选择 3 张牌，附魔皇室认证。
/// </summary>
[RegisterRelic(typeof(FeiyapRelicPool))]
public sealed class GouChenYi : ZhanXingGuanRelicBase
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CardsVar(3),
        new StringVar("Enchantment", ModelDb.Enchantment<RoyallyApproved>().Title.GetFormattedText()),
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
        HoverTipFactory.FromEnchantment<RoyallyApproved>();

    public override Task AfterObtained() =>
        FeiyapEnchantRelicCmd.EnchantFromDeck<RoyallyApproved>(Owner, 3);
}

/// <summary>
/// 天市右垣七：从角色卡池选择 1 张稀有卡加入牌组。
/// </summary>
[RegisterRelic(typeof(FeiyapRelicPool))]
public sealed class TianShiYouYuanQi : ZhanXingGuanRelicBase
{
    private const int OfferCount = 3;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(1)];

    public override bool HasUponPickupEffect => true;

    public override async Task AfterObtained()
    {
        var options = new CardCreationOptions(
            [Owner.Character.CardPool],
            CardCreationSource.Other,
            CardRarityOddsType.Uniform,
            c => c.Rarity == CardRarity.Rare)
            .WithFlags(CardCreationFlags.NoUpgradeRoll);

        var offeredCards = CardFactory.CreateForReward(Owner, OfferCount, options)
            .Select(result => result.Card)
            .ToList();

        if (offeredCards.Count == 0)
        {
            return;
        }

        var chosenCard = await CardSelectCmd.FromChooseACardScreen(
            new BlockingPlayerChoiceContext(),
            offeredCards,
            Owner,
            canSkip: false);

        if (chosenCard == null)
        {
            return;
        }

        CardCmd.PreviewCardPileAdd(await CardPileCmd.Add(chosenCard, PileType.Deck));
    }
}

/// <summary>
/// 天津四：选择 1 张能力牌附魔注能。
/// </summary>
[RegisterRelic(typeof(FeiyapRelicPool))]
public sealed class TianJinSi : ZhanXingGuanRelicBase
{
    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
        HoverTipFactory.FromEnchantment<FeiyapInfusionEnchantment>();

    public override bool HasUponPickupEffect => true;

    public override Task AfterObtained() =>
        FeiyapEnchantRelicCmd.EnchantFromDeck<FeiyapInfusionEnchantment>(Owner, 1);
}

/// <summary>
/// 织女一：选择 1 张攻击牌附魔强袭。
/// </summary>
[RegisterRelic(typeof(FeiyapRelicPool))]
public sealed class ZhiNvYi : ZhanXingGuanRelicBase
{
    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
        HoverTipFactory.FromEnchantment<FeiyapAssaultEnchantment>();

    public override bool HasUponPickupEffect => true;

    public override Task AfterObtained() =>
        FeiyapEnchantRelicCmd.EnchantFromDeck<FeiyapAssaultEnchantment>(Owner, 1);
}

/// <summary>
/// 河鼓二：选择 1 张技能牌附魔重燃。
/// </summary>
[RegisterRelic(typeof(FeiyapRelicPool))]
public sealed class HeGuEr : ZhanXingGuanRelicBase
{
    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
        HoverTipFactory.FromEnchantment<FeiyapRekindleEnchantment>();

    public override bool HasUponPickupEffect => true;

    public override Task AfterObtained() =>
        FeiyapEnchantRelicCmd.EnchantFromDeck<FeiyapRekindleEnchantment>(Owner, 1);
}

/// <summary>
/// 天狼星：战斗开始时额外抽 1 张牌；洗牌时抽 2 张牌。
/// </summary>
[RegisterRelic(typeof(FeiyapRelicPool))]
public sealed class TianLangXing : ZhanXingGuanRelicBase
{
    private const int CombatStartDraw = 1;
    private const int ShuffleDraw = 2;

    public override string FlashSfx => "event:/sfx/ui/relic_activate_draw";

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CardsVar(CombatStartDraw),
        new DynamicVar("ShuffleDraw", ShuffleDraw),
    ];

    public override async Task BeforeHandDraw(
        Player player,
        PlayerChoiceContext choiceContext,
        ICombatState combatState)
    {
        if (player != Owner || Owner.PlayerCombatState.TurnNumber != 1)
        {
            return;
        }

        Flash();
        await CardPileCmd.Draw(choiceContext, CombatStartDraw, Owner);
    }

    public override async Task AfterShuffle(PlayerChoiceContext choiceContext, Player shuffler)
    {
        if (shuffler != Owner)
        {
            return;
        }

        Flash();
        await CardPileCmd.Draw(choiceContext, ShuffleDraw, Owner);
    }
}

/// <summary>
/// 南河三：每回合首次打出攻击/技能/能力牌时各抽 1 张牌。
/// </summary>
[RegisterRelic(typeof(FeiyapRelicPool))]
public sealed class NanHeSan : ZhanXingGuanRelicBase
{
    private bool _drewFromAttack;
    private bool _drewFromSkill;
    private bool _drewFromPower;

    public override string FlashSfx => "event:/sfx/ui/relic_activate_draw";

    protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(1)];

    public override Task BeforeSideTurnStart(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        if (!participants.Contains(Owner.Creature))
        {
            return Task.CompletedTask;
        }

        _drewFromAttack = false;
        _drewFromSkill = false;
        _drewFromPower = false;
        return Task.CompletedTask;
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (!CombatManager.Instance.IsInProgress || cardPlay.Card.Owner != Owner)
        {
            return;
        }

        var shouldDraw = cardPlay.Card.Type switch
        {
            CardType.Attack => !_drewFromAttack,
            CardType.Skill => !_drewFromSkill,
            CardType.Power => !_drewFromPower,
            _ => false,
        };

        if (!shouldDraw)
        {
            return;
        }

        switch (cardPlay.Card.Type)
        {
            case CardType.Attack:
                _drewFromAttack = true;
                break;
            case CardType.Skill:
                _drewFromSkill = true;
                break;
            case CardType.Power:
                _drewFromPower = true;
                break;
        }

        Flash();
        await CardPileCmd.Draw(choiceContext, 1, Owner);
    }

    public override Task AfterCombatEnd(CombatRoom _)
    {
        _drewFromAttack = false;
        _drewFromSkill = false;
        _drewFromPower = false;
        return Task.CompletedTask;
    }
}

/// <summary>
/// 参宿四：回合开始时获得 1 耗能；第一回合无法打出攻击牌。
/// </summary>
[RegisterRelic(typeof(FeiyapRelicPool))]
public sealed class ShenSuSi : ZhanXingGuanRelicBase
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new EnergyVar(1)];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
        [HoverTipFactory.ForEnergy(this)];

    public override async Task AfterEnergyReset(Player player)
    {
        if (player != Owner)
        {
            return;
        }

        Flash();
        await PlayerCmd.GainEnergy(DynamicVars.Energy.BaseValue, Owner);
    }

    public override bool ShouldPlay(CardModel card, AutoPlayType _)
    {
        if (card.Owner != Owner || Owner.PlayerCombatState.TurnNumber > 1)
        {
            return true;
        }

        return card.Type != CardType.Attack;
    }
}

/// <summary>
/// 凤凰座 A：移除最多 5 张牌，每移除 1 张失去 2 点最大生命。
/// </summary>
[RegisterRelic(typeof(FeiyapRelicPool))]
public sealed class FenHuangZuoA : ZhanXingGuanRelicBase
{
    private const int MaxRemove = 5;
    private const int MaxHpLossPerCard = 2;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CardsVar(MaxRemove),
        new MaxHpVar(MaxHpLossPerCard),
    ];

    public override bool HasUponPickupEffect => true;

    public override async Task AfterObtained()
    {
        var removedCards = (await CardSelectCmd.FromDeckForRemoval(
            Owner,
            new CardSelectorPrefs(CardSelectorPrefs.RemoveSelectionPrompt, 0, MaxRemove)
            {
                Cancelable = true,
            })).ToList();

        if (removedCards.Count == 0)
        {
            return;
        }

        await CardPileCmd.RemoveFromDeck(removedCards);

        var maxHpLoss = removedCards.Count * MaxHpLossPerCard;
        await CreatureCmd.LoseMaxHp(
            new ThrowingPlayerChoiceContext(),
            Owner.Creature,
            maxHpLoss,
            isFromCard: false);
    }
}

/// <summary>
/// 十月天龙座流星雨：抽到状态牌时使其消耗，对随机敌人造成 6 点伤害。
/// </summary>
[RegisterRelic(typeof(FeiyapRelicPool))]
public sealed class ShiYueTianLongZuoLiuXingYu : ZhanXingGuanRelicBase
{
    private const int DamageAmount = 6;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DamageVar(DamageAmount, ValueProp.Unpowered)];

    public override async Task AfterCardDrawn(
        PlayerChoiceContext choiceContext,
        CardModel card,
        bool fromHandDraw)
    {
        if (!CombatManager.Instance.IsInProgress
            || card.Owner != Owner
            || card.Type != CardType.Status
            || card.Pile?.Type != PileType.Hand)
        {
            return;
        }

        var target = Owner.RunState.Rng.CombatTargets.NextItem(Owner.Creature.CombatState.HittableEnemies);
        if (target == null)
        {
            return;
        }

        Flash();
        await CardCmd.Exhaust(choiceContext, card);
        await CreatureCmd.Damage(
            choiceContext,
            [target],
            DynamicVars.Damage,
            Owner.Creature);
    }
}
