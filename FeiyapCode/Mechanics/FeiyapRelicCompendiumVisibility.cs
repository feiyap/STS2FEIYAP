using MegaCrit.Sts2.Core.Models;

namespace Feiyap.Mechanics;

/// <summary>
/// 标记不在遗物图鉴中显示的遗物。
/// </summary>
public interface IFeiyapHiddenFromRelicCompendium;

internal static class FeiyapRelicCompendiumVisibility
{
    internal static bool ShouldShowInRelicCompendium(RelicModel relic) =>
        relic is not IFeiyapHiddenFromRelicCompendium;

    internal static IEnumerable<RelicModel> FilterForRelicCompendium(IEnumerable<RelicModel> relics) =>
        relics.Where(FeiyapRelicCompendiumVisibility.ShouldShowInRelicCompendium);
}
