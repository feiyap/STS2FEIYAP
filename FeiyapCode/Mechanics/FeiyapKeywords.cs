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

    /// <summary>体内灼烧关键词 id。</summary>
    public const string InternalBurnId = "FEIYAP_KEYWORD_INTERNAL_BURN";

    /// <summary>星座关键词 id。</summary>
    public const string ConstellationId = "FEIYAP_KEYWORD_CONSTELLATION";

    /// <summary>残心关键词 id。</summary>
    public const string ZanxinId = "FEIYAP_KEYWORD_ZANXIN";

    /// <summary>黑洞关键词 id。</summary>
    public const string VoidHoleId = "FEIYAP_KEYWORD_VOID_HOLE";

    /// <summary>热寂关键词 id。</summary>
    public const string HeatDeathId = "FEIYAP_KEYWORD_HEAT_DEATH";

    /// <summary>塔罗-正位关键词 id。</summary>
    public const string TarotUprightId = "FEIYAP_KEYWORD_TAROT_UPRIGHT";

    /// <summary>塔罗-逆位关键词 id。</summary>
    public const string TarotReversedId = "FEIYAP_KEYWORD_TAROT_REVERSED";

    /// <summary>居合关键词。</summary>
    public static CardKeyword Iaido { get; private set; }

    /// <summary>体内灼烧关键词。</summary>
    public static CardKeyword InternalBurn { get; private set; }

    /// <summary>星座关键词。</summary>
    public static CardKeyword Constellation { get; private set; }

    /// <summary>残心关键词。</summary>
    public static CardKeyword Zanxin { get; private set; }

    /// <summary>黑洞关键词。</summary>
    public static CardKeyword VoidHole { get; private set; }

    /// <summary>热寂关键词。</summary>
    public static CardKeyword HeatDeath { get; private set; }

    /// <summary>塔罗-正位关键词。</summary>
    public static CardKeyword TarotUpright { get; private set; }

    /// <summary>塔罗-逆位关键词。</summary>
    public static CardKeyword TarotReversed { get; private set; }

    public static void Register(ModKeywordRegistry registry)
    {
        Iaido = registry.RegisterCardKeywordOwnedByLocNamespace("IAIDO").CardKeywordValue;
        InternalBurn = registry.RegisterCardKeywordOwnedByLocNamespace("INTERNAL_BURN").CardKeywordValue;
        Constellation = registry.RegisterCardKeywordOwnedByLocNamespace("CONSTELLATION").CardKeywordValue;
        Zanxin = registry.RegisterCardKeywordOwnedByLocNamespace("ZANXIN").CardKeywordValue;
        VoidHole = registry.RegisterCardKeywordOwnedByLocNamespace("VOID_HOLE").CardKeywordValue;
        HeatDeath = registry.RegisterCardKeywordOwnedByLocNamespace("HEAT_DEATH").CardKeywordValue;
        TarotUpright = registry.RegisterCardKeywordOwnedByLocNamespace("TAROT_UPRIGHT").CardKeywordValue;
        TarotReversed = registry.RegisterCardKeywordOwnedByLocNamespace("TAROT_REVERSED").CardKeywordValue;
    }
}
