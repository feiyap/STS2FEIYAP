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
}
