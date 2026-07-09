using STS2RitsuLib.Scaffolding.Content;

namespace Feiyap.Powers;

internal static class FeiyapPowerAssets
{
    internal static PowerAssetProfile For(string powerName) => ForSharedIcon(powerName, powerName);

    internal static PowerAssetProfile ForSharedIcon(string powerName, string iconSourceName) => new(
        IconPath: $"{Entry.ResPath}/images/powers/{iconSourceName}.png",
        BigIconPath: $"{Entry.ResPath}/images/powers/{iconSourceName}.png");
}
