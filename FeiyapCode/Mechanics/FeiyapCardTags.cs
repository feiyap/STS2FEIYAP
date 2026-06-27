using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.CardTags;

namespace Feiyap.Mechanics;

/// <summary>
/// 绯夜氏模组自定义卡牌标签。
/// </summary>
public static class FeiyapCardTags
{
    /// <summary>居合标签。</summary>
    public static CardTag Iaido { get; private set; }

    /// <summary>体内灼烧标签。</summary>
    public static CardTag InternalBurn { get; private set; }

    /// <summary>星座标签。</summary>
    public static CardTag Constellation { get; private set; }

    /// <summary>塔罗标签。</summary>
    public static CardTag Tarot { get; private set; }

    public static void Register(ModCardTagRegistry registry)
    {
        Iaido = registry.RegisterOwned("IAIDO").CardTagValue;
        InternalBurn = registry.RegisterOwned("INTERNAL_BURN").CardTagValue;
        Constellation = registry.RegisterOwned("CONSTELLATION").CardTagValue;
        Tarot = registry.RegisterOwned("TAROT").CardTagValue;
    }

    public static bool HasIaido(CardModel? card) =>
        card != null && card.Tags.Contains(Iaido);

    public static bool HasInternalBurn(CardModel? card) =>
        card != null && card.Tags.Contains(InternalBurn);

    public static bool HasConstellation(CardModel? card) =>
        card != null && card.Tags.Contains(Constellation);

    public static bool HasTarot(CardModel? card) =>
        card != null && card.Tags.Contains(Tarot);
}
