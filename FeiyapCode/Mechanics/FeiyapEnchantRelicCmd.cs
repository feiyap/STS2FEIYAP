using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Vfx;

namespace Feiyap.Mechanics;

/// <summary>
/// 遗物拾取时从牌组选牌并附魔的共用逻辑。
/// </summary>
internal static class FeiyapEnchantRelicCmd
{
    internal static async Task EnchantFromDeck<TEnchantment>(
        Player player,
        int selectCount,
        decimal enchantAmount = 1m)
        where TEnchantment : EnchantmentModel
    {
        var enchantment = ModelDb.Enchantment<TEnchantment>();
        var prefs = new CardSelectorPrefs(CardSelectorPrefs.EnchantSelectionPrompt, 0, selectCount)
        {
            Cancelable = false,
            RequireManualConfirmation = true,
        };

        foreach (var card in await CardSelectCmd.FromDeckForEnchantment(
                     player,
                     enchantment,
                     selectCount,
                     prefs))
        {
            CardCmd.Enchant(enchantment.ToMutable(), card, enchantAmount);
            ShowEnchantVfx(card);
            CardCmd.Preview(card);
        }
    }

    private static void ShowEnchantVfx(CardModel card)
    {
        var vfx = NCardEnchantVfx.Create(card);
        if (vfx != null)
        {
            NRun.Instance?.GlobalUi.CardPreviewContainer.AddChildSafely(vfx);
        }
    }
}
