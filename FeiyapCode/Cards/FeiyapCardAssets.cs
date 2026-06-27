using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Utils;

namespace Feiyap.Cards;

internal static class FeiyapCardAssets
{
    private const string CardsRoot = $"{Entry.ResPath}/images/cards";
    private static readonly string WipPortraitPath = $"{CardsRoot}/WIP.png";

    internal static CardAssetProfile For(string cardName) =>
        new(PortraitPath: ResolvePortraitPath(cardName));

    internal static string ResolvePortraitPath(string cardName)
    {
        var portraitPath = $"{CardsRoot}/{cardName}.png";
        return GodotResourcePath.ResourceExists(portraitPath)
            ? portraitPath
            : WipPortraitPath;
    }
}
