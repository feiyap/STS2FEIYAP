using Feiyap.Mechanics;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Screens.RelicCollection;
using STS2RitsuLib.Patching.Models;

namespace Feiyap.Patches;

/// <summary>
/// 遗物图鉴：过滤标记为隐藏的遗物条目。
/// </summary>
public sealed class FeiyapRelicCompendiumLoadRelicNodesPatch : IPatchMethod
{
    public static string PatchId => "feiyap_relic_compendium_load_relic_nodes";

    public static string Description => "遗物图鉴隐藏指定遗物";

    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets() =>
    [
        new(typeof(NRelicCollectionCategory), nameof(NRelicCollectionCategory.LoadRelicNodes), [
            typeof(IEnumerable<RelicModel>),
            typeof(HashSet<RelicModel>),
            typeof(HashSet<RelicModel>)
        ])
    ];

    public static void Prefix(ref IEnumerable<RelicModel> relics) =>
        relics = FeiyapRelicCompendiumVisibility.FilterForRelicCompendium(relics);
}

/// <summary>
/// 遗物图鉴：同步过滤用于检视导航的遗物列表。
/// </summary>
public sealed class FeiyapRelicCompendiumAddRelicsPatch : IPatchMethod
{
    public static string PatchId => "feiyap_relic_compendium_add_relics";

    public static string Description => "遗物图鉴检视列表隐藏指定遗物";

    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets() =>
    [
        new(typeof(NRelicCollection), nameof(NRelicCollection.AddRelics), [
            typeof(IEnumerable<RelicModel>)
        ])
    ];

    public static void Prefix(ref IEnumerable<RelicModel> relics) =>
        relics = FeiyapRelicCompendiumVisibility.FilterForRelicCompendium(relics);
}
