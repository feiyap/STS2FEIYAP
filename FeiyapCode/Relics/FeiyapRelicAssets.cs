using STS2RitsuLib.Scaffolding.Content;

namespace Feiyap.Relics;

internal static class FeiyapRelicAssets
{
    internal static RelicAssetProfile For(string relicName) => new(
        IconPath: $"{Entry.ResPath}/images/relics/{relicName}.png",
        IconOutlinePath: $"{Entry.ResPath}/images/relics/{relicName}.png",
        BigIconPath: $"{Entry.ResPath}/images/relics/{relicName}.png");
}
