using System.Collections.Generic;
using System.Threading.Tasks;
using Feiyap.Cards.Ancients;
using Feiyap.Characters;
using Feiyap.Mechanics;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Feiyap.Relics;

public abstract class MerryWitchBase : ModRelicTemplate, IFeiyapIaidoGainMultiplier
{
    protected abstract decimal AlternateBonusMultiplier { get; }

    public override RelicRarity Rarity => RelicRarity.Event;

    /// <summary>
    /// 遗物加成仅对持有者本人的角色实体生效（联机下不误作用于队友）。
    /// </summary>
    private bool IsRelicOwnerCreature(Creature? creature) =>
        creature != null && creature == Owner.Creature;

    /// <summary>
    /// 出牌结算时根据上一张攻击/技能牌判断是否处于交替出牌。
    /// 不能用 AlternateBonusActive：它在 AfterCardPlayed 才更新，且同类型连打时会短暂残留 true。
    /// </summary>
    private bool ShouldApplyAlternateBonus(CardModel? cardSource, CardPlay? cardPlay = null)
    {
        if (cardSource?.Owner != Owner)
        {
            return false;
        }

        if (cardPlay != null && cardPlay.Card.Owner != Owner)
        {
            return false;
        }

        var type = cardSource.Type;
        if (type is not (CardType.Attack or CardType.Skill))
        {
            return false;
        }

        var lastPlayedType = FeiyapCombatTracker.Get(Owner).LastPlayedType;
        return lastPlayedType is CardType.Attack or CardType.Skill && lastPlayedType != type;
    }

    public decimal ModifyIaidoGainMultiplicative(in FeiyapIaidoGainContext context, decimal amount)
    {
        if (!IsRelicOwnerCreature(context.Creature)
            || !ShouldApplyAlternateBonus(context.CardSource, context.CardPlay))
        {
            return amount;
        }

        return amount * AlternateBonusMultiplier;
    }

    public override decimal ModifyDamageMultiplicative(
        Creature? target,
        decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource,
        CardPlay? cardPlay)
    {
        if (!IsRelicOwnerCreature(dealer)
            || !ShouldApplyAlternateBonus(cardSource, cardPlay))
        {
            return 1m;
        }

        return AlternateBonusMultiplier;
    }

    public override decimal ModifyBlockMultiplicative(
        Creature target,
        decimal block,
        ValueProp props,
        CardModel? cardSource,
        CardPlay? cardPlay)
    {
        if (!IsRelicOwnerCreature(target)
            || !ShouldApplyAlternateBonus(cardSource, cardPlay))
        {
            return 1m;
        }

        return AlternateBonusMultiplier;
    }
}

[RegisterRelic(typeof(FeiyapRelicPool))]
public sealed class MerryWitch : MerryWitchBase
{
    protected override decimal AlternateBonusMultiplier => 1.25m;

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
        [HoverTipFactory.FromCard(ModelDb.Card<WorldXxi>())];

    public override RelicAssetProfile AssetProfile => FeiyapRelicAssets.For(nameof(MerryWitch));

    public override async Task AfterObtained()
    {
        if (FeiyapQuestRewards.SuppressQuestRelicObtainEffects)
        {
            return;
        }

        await FeiyapQuestRewards.GainAncientCard<WorldXxi>(Owner, upgraded: false);
    }
}

[RegisterRelic(typeof(FeiyapRelicPool))]
public sealed class KuangXiaoMoNv : MerryWitchBase
{
    protected override decimal AlternateBonusMultiplier => 1.5m;

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
        [HoverTipFactory.FromCard(ModelDb.Card<WorldXxi>())];

    public override RelicAssetProfile AssetProfile => FeiyapRelicAssets.For(nameof(KuangXiaoMoNv));

    public override async Task AfterObtained()
    {
        if (FeiyapQuestRewards.SuppressQuestRelicObtainEffects)
        {
            return;
        }

        await FeiyapQuestRewards.GainAncientCard<WorldXxi>(Owner, upgraded: true);
    }
}
