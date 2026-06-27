using MegaCrit.Sts2.Core.Entities.Cards;
using STS2RitsuLib.Scaffolding.Content;

namespace Feiyap.Cards;

/// <summary>
/// 绯夜氏卡牌基类：卡图缺失时回退到 WIP.png。
/// </summary>
public abstract class FeiyapCardTemplate(
    int baseCost,
    CardType type,
    CardRarity rarity,
    TargetType target,
    bool showInCardLibrary = true)
    : ModCardTemplate(baseCost, type, rarity, target, showInCardLibrary)
{
    public override CardAssetProfile AssetProfile => FeiyapCardAssets.For(GetType().Name);
}
