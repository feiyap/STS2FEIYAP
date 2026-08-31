using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.CommonUi;

namespace Feiyap.Mechanics;

/// <summary>
/// 卡牌变化时保留升级与附魔（太阳/月亮互变等）。
/// </summary>
internal static class FeiyapCardTransformCmd
{
    public static async Task TransformPreserving<TReplacement>(CardModel original)
        where TReplacement : CardModel
    {
        if (original.Pile?.Type != PileType.Hand || original.CombatState == null)
        {
            return;
        }

        var replacement = original.CombatState.CreateCard<TReplacement>(original.Owner);
        CopyUpgradeAndEnchantment(original, replacement);
        await CardCmd.Transform(original, replacement, CardPreviewStyle.None);
    }

    private static void CopyUpgradeAndEnchantment(CardModel source, CardModel dest)
    {
        if (source.IsUpgraded)
        {
            CardCmd.Upgrade(dest);
        }

        if (source.Enchantment == null)
        {
            return;
        }

        var enchantment = (EnchantmentModel)source.Enchantment.MutableClone();
        dest.EnchantInternal(enchantment, enchantment.Amount);
        enchantment.ModifyCard();
        dest.FinalizeUpgradeInternal();
    }
}
