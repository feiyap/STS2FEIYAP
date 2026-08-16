using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Utils;

namespace Feiyap.Cards;

internal static class FeiyapCardAssets
{
    private const string CardsRoot = $"{Entry.ResPath}/images/cards";
    private static readonly string WipPortraitPath = $"{CardsRoot}/WIP.png";

    internal static CardAssetProfile For(string cardName, bool reversed = false) =>
        new(PortraitPath: ResolvePortraitPath(cardName, reversed));

    /// <summary>
    /// 是否存在独立的逆位卡图资源。
    /// </summary>
    internal static bool HasReversedPortrait(string cardName) =>
        GodotResourcePath.ResourceExists($"{CardsRoot}/{cardName}_Reversed.png");

    /// <summary>
    /// 解析卡图路径。逆位优先 <c>{name}_Reversed.png</c>，缺失时回退正位图，再回退 WIP。
    /// </summary>
    internal static string ResolvePortraitPath(string cardName, bool reversed = false)
    {
        if (reversed)
        {
            var reversedPath = $"{CardsRoot}/{cardName}_Reversed.png";
            if (GodotResourcePath.ResourceExists(reversedPath))
            {
                return reversedPath;
            }
        }

        var portraitPath = $"{CardsRoot}/{cardName}.png";
        return GodotResourcePath.ResourceExists(portraitPath)
            ? portraitPath
            : WipPortraitPath;
    }
}
