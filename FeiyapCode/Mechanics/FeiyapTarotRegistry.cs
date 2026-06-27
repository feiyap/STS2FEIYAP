using System.Collections.Generic;
using Feiyap.Cards.Tarot;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;

namespace Feiyap.Mechanics;

/// <summary>
/// 塔罗牌注册表：供 XXI-世界 等效果展示并生成塔罗牌。
/// </summary>
public static class FeiyapTarotRegistry
{
    private static readonly List<FeiyapTarotCardFactory> Factories = [];

    public static void Register(FeiyapTarotCardFactory factory)
    {
        if (!Factories.Contains(factory))
        {
            Factories.Add(factory);
        }
    }

    public static IEnumerable<CardModel> CreateSelectionPool(Player player, CardModel? exclude = null)
    {
        foreach (var factory in Factories)
        {
            var card = factory(player);
            if (exclude != null && card.GetType() == exclude.GetType())
            {
                continue;
            }

            yield return card;
        }
    }
}

public delegate CardModel FeiyapTarotCardFactory(Player player);
