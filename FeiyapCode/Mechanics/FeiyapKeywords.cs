using MegaCrit.Sts2.Core.Entities.Cards;
using STS2RitsuLib.Keywords;

namespace Feiyap.Mechanics;

/// <summary>
/// 飞鸦模组自定义卡牌关键词。
/// </summary>
public static class FeiyapKeywords
{
    /// <summary>居合关键词 id（用于 RegisteredKeywordIds 等悬停提示引用）。</summary>
    public const string IaidoId = "FEIYAP_KEYWORD_IAIDO";

    /// <summary>星座关键词 id。</summary>
    public const string ConstellationId = "FEIYAP_KEYWORD_CONSTELLATION";

    /// <summary>残心关键词 id。</summary>
    public const string ZanxinId = "FEIYAP_KEYWORD_ZANXIN";

    /// <summary>塔罗-正位关键词 id。</summary>
    public const string TarotUprightId = "FEIYAP_KEYWORD_TAROT_UPRIGHT";

    /// <summary>塔罗-逆位关键词 id。</summary>
    public const string TarotReversedId = "FEIYAP_KEYWORD_TAROT_REVERSED";

    /// <summary>完美居合关键词 id。</summary>
    public const string PerfectIaidoId = "FEIYAP_KEYWORD_PERFECT_IAIDO";

    /// <summary>居合关键词。</summary>
    public static CardKeyword Iaido { get; private set; }

    /// <summary>星座关键词。</summary>
    public static CardKeyword Constellation { get; private set; }

    /// <summary>残心关键词。</summary>
    public static CardKeyword Zanxin { get; private set; }

    /// <summary>塔罗-正位关键词。</summary>
    public static CardKeyword TarotUpright { get; private set; }

    /// <summary>塔罗-逆位关键词。</summary>
    public static CardKeyword TarotReversed { get; private set; }

    /// <summary>完美居合关键词。</summary>
    public static CardKeyword PerfectIaido { get; private set; }

    public static void Register(ModKeywordRegistry registry)
    {
        Iaido = registry.RegisterCardKeywordOwnedByLocNamespace("IAIDO").CardKeywordValue;
        Constellation = registry.RegisterCardKeywordOwnedByLocNamespace("CONSTELLATION").CardKeywordValue;
        Zanxin = registry.RegisterCardKeywordOwnedByLocNamespace("ZANXIN").CardKeywordValue;
        TarotUpright = registry.RegisterCardKeywordOwnedByLocNamespace("TAROT_UPRIGHT").CardKeywordValue;
        TarotReversed = registry.RegisterCardKeywordOwnedByLocNamespace("TAROT_REVERSED").CardKeywordValue;
        PerfectIaido = registry.RegisterCardKeywordOwnedByLocNamespace("PERFECT_IAIDO").CardKeywordValue;
    }
}
