using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Feiyap.Mechanics;

/// <summary>
/// 居合获得流程中的修正上下文。
/// </summary>
public readonly struct FeiyapIaidoGainContext
{
    public required PlayerChoiceContext ChoiceContext { get; init; }

    public required Creature Creature { get; init; }

    /// <summary>卡牌或效果声明的基础居合量。</summary>
    public required decimal BaseAmount { get; init; }

    /// <summary>敏捷与乘算修正后的居合量（加算修正前）。</summary>
    public required decimal ScaledAmount { get; init; }

    public required ValueProp Props { get; init; }

    public CardModel? CardSource { get; init; }

    public CardPlay? CardPlay { get; init; }

    /// <summary>卡牌预览计算时为 true；此时残心可显示加成但不登记消耗。</summary>
    public bool IsPreview { get; init; }

    /// <summary>
    /// 本次获得视为打出卡牌产生，但调用方拿不到 <see cref="CardPlay"/>。
    /// 银碗盛雪在 <c>BeforeBlockGained</c> 中把格挡转为居合时使用：残心仍要加算并登记消耗。
    /// </summary>
    public bool CountsAsCardPlay { get; init; }
}
