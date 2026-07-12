using Godot;

using Feiyap.Relics;

using MegaCrit.Sts2.Core.Events;

using MegaCrit.Sts2.Core.Extensions;

using MegaCrit.Sts2.Core.Models.Acts;

using STS2RitsuLib.Interop.AutoRegistration;

using STS2RitsuLib.Scaffolding.Content;



namespace Feiyap.Ancients;



/// <summary>
/// 先古之民：斩星官。
/// 本地化 Entry 为 <c>FEIYAP_EVENT_ZHAN_XING_GUAN</c>（非 ModAnalyzers 推断的 ANCIENT 前缀）。
/// </summary>
[RegisterActAncient(typeof(Glory))]
public sealed class ZhanXingGuan : ModAncientEventTemplate

{

    private const string SceneRoot = $"{Entry.ResPath}/scenes/ancients";

    private const string ImageRoot = $"{Entry.ResPath}/images/ancients";



    public override Color ButtonColor => new(0.05f, 0.04f, 0.14f, 0.55f);



    public override Color DialogueColor => new("1a1a3e");



    public override EventAssetProfile AssetProfile => new(

        BackgroundScenePath: $"{SceneRoot}/zhan_xing_guan.tscn");



    public override AncientEventPresentationAssetProfile AncientPresentationAssetProfile => new(

        MapIconPath: $"{ImageRoot}/zhan_xing_guan_map.png",

        MapIconOutlinePath: $"{ImageRoot}/zhan_xing_guan_map_outline.png",

        RunHistoryIconPath: $"{ImageRoot}/zhan_xing_guan_history.png",

        RunHistoryIconOutlinePath: $"{ImageRoot}/zhan_xing_guan_history_outline.png");



    // 选项一：附魔类遗物
    private IReadOnlyList<EventOption> Pool1 =>
    [
        CreateModRelicOption<TianJinSi>(),
        CreateModRelicOption<ZhiNvYi>(),
        CreateModRelicOption<HeGuEr>(),
    ];

    // 选项二：抽牌/能量类遗物
    private IReadOnlyList<EventOption> Pool2 =>
    [
        CreateModRelicOption<TianLangXing>(),
        CreateModRelicOption<NanHeSan>(),
        CreateModRelicOption<ShenSuSi>(),
    ];

    // 选项三：牌组变更/特殊机制类遗物
    private IReadOnlyList<EventOption> Pool3 =>
    [
        CreateModRelicOption<GouChenYi>(),
        CreateModRelicOption<TianShiYouYuanQi>(),
        CreateModRelicOption<FenHuangZuoA>(),
        CreateModRelicOption<ShiYueTianLongZuoLiuXingYu>(),
    ];

    public override IEnumerable<EventOption> AllPossibleOptions =>
        [.. Pool1, .. Pool2, .. Pool3];

    protected override IReadOnlyList<EventOption> GenerateInitialOptions() =>
    [
        Rng.NextItem(Pool1)!,
        Rng.NextItem(Pool2)!,
        Rng.NextItem(Pool3)!,
    ];

}


