using System.Reflection;
using System.Runtime.CompilerServices;
using Feiyap.Cards.Tarot;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.Screens;
using MegaCrit.Sts2.addons.mega_text;
using STS2RitsuLib.Patching.Models;

namespace Feiyap.Patches;

/// <summary>
/// 在卡牌检查界面「查看升级」旁追加「查看正逆位」，用于切换塔罗正/逆位卡图。
/// </summary>
public sealed class FeiyapInspectTarotOrientationReadyPatch : IPatchMethod
{
    public static string PatchId => "feiyap_inspect_tarot_orientation_ready";

    public static string Description => "检查界面追加查看正逆位勾选框";

    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets() =>
    [
        new(typeof(NInspectCardScreen), "_Ready")
    ];

    public static void Postfix(NInspectCardScreen __instance) =>
        FeiyapInspectTarotOrientationUi.EnsureCreated(__instance);
}

/// <summary>
/// 打开检查界面时启用正逆位勾选框。
/// </summary>
public sealed class FeiyapInspectTarotOrientationOpenPatch : IPatchMethod
{
    public static string PatchId => "feiyap_inspect_tarot_orientation_open";

    public static string Description => "打开检查界面时启用正逆位勾选框";

    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets() =>
    [
        new(typeof(NInspectCardScreen), nameof(NInspectCardScreen.Open),
            [typeof(List<CardModel>), typeof(int), typeof(bool)])
    ];

    public static void Postfix(NInspectCardScreen __instance) =>
        FeiyapInspectTarotOrientationUi.OnOpen(__instance);
}

/// <summary>
/// 关闭检查界面时禁用正逆位勾选框。
/// </summary>
public sealed class FeiyapInspectTarotOrientationClosePatch : IPatchMethod
{
    public static string PatchId => "feiyap_inspect_tarot_orientation_close";

    public static string Description => "关闭检查界面时禁用正逆位勾选框";

    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets() =>
    [
        new(typeof(NInspectCardScreen), nameof(NInspectCardScreen.Close))
    ];

    public static void Postfix(NInspectCardScreen __instance) =>
        FeiyapInspectTarotOrientationUi.OnClose(__instance);
}

/// <summary>
/// 切换检查卡牌时同步正逆位勾选框可见性与状态。
/// </summary>
public sealed class FeiyapInspectTarotOrientationSetCardPatch : IPatchMethod
{
    public static string PatchId => "feiyap_inspect_tarot_orientation_set_card";

    public static string Description => "切换检查卡牌时同步正逆位勾选框";

    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets() =>
    [
        new(typeof(NInspectCardScreen), "SetCard", [typeof(int)])
    ];

    public static void Postfix(NInspectCardScreen __instance) =>
        FeiyapInspectTarotOrientationUi.OnSetCard(__instance);
}

/// <summary>
/// 刷新检查卡面后应用正/逆位卡图覆盖。
/// </summary>
public sealed class FeiyapInspectTarotOrientationUpdateDisplayPatch : IPatchMethod
{
    public static string PatchId => "feiyap_inspect_tarot_orientation_update_display";

    public static string Description => "检查卡面刷新后应用正逆位卡图";

    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets() =>
    [
        new(typeof(NInspectCardScreen), "UpdateCardDisplay")
    ];

    public static void Postfix(NInspectCardScreen __instance) =>
        FeiyapInspectTarotOrientationUi.ApplyPortraitOverride(__instance);
}

/// <summary>
/// 检查界面塔罗正逆位勾选框的创建、布局与卡图应用。
/// </summary>
internal static class FeiyapInspectTarotOrientationUi
{
    private const string TickboxName = "FeiyapOrientation";
    private const string StandaloneLabelName = "FeiyapOrientationLabel";
    private const float HorizontalGap = 48f;

    private static readonly ConditionalWeakTable<NInspectCardScreen, State> States = new();

    private static readonly FieldInfo? CardField = AccessTools.Field(typeof(NInspectCardScreen), "_card");
    private static readonly FieldInfo? CardsField = AccessTools.Field(typeof(NInspectCardScreen), "_cards");
    private static readonly FieldInfo? IndexField = AccessTools.Field(typeof(NInspectCardScreen), "_index");
    private static readonly FieldInfo? UpgradeTickboxField =
        AccessTools.Field(typeof(NInspectCardScreen), "_upgradeTickbox");

    private sealed class State
    {
        public NTickbox Tickbox = null!;
        public MegaLabel? StandaloneLabel;
        public Vector2 UpgradeBasePosition;
        public Vector2 LabelBasePosition;
        public Vector2 LabelOffsetFromUpgrade;
        public bool LabelIsChildOfUpgrade;
        public bool Created;
    }

    public static void EnsureCreated(NInspectCardScreen screen)
    {
        if (States.TryGetValue(screen, out var existing) && existing.Created)
        {
            return;
        }

        var upgrade = UpgradeTickboxField?.GetValue(screen) as NTickbox
            ?? screen.GetNodeOrNull<NTickbox>("%Upgrade");
        var label = screen.GetNodeOrNull<MegaLabel>("%ShowUpgradeLabel");
        if (upgrade == null || label == null || upgrade.GetParent() is not Node parent)
        {
            return;
        }

        var state = States.GetOrCreateValue(screen);
        state.UpgradeBasePosition = upgrade.Position;
        state.LabelBasePosition = label.Position;
        state.LabelOffsetFromUpgrade = label.Position - upgrade.Position;
        state.LabelIsChildOfUpgrade = label.GetParent() == upgrade;

        // 勿清除子节点 UniqueNameInOwner：NTickbox / NHotkeyIcon 依赖 %TickboxVisuals 等路径。
        // 必须在 AddChild（触发 _Ready）之前把子树 Owner 指回副本根，否则 % 查找会失败。
        var tickbox = (NTickbox)upgrade.Duplicate((int)Node.DuplicateFlags.Scripts);
        tickbox.Name = TickboxName;
        tickbox.UniqueNameInOwner = false;
        DisconnectAll(tickbox, NTickbox.SignalName.Toggled);
        ReownDescendants(tickbox, tickbox);

        parent.AddChild(tickbox);

        MegaLabel? orientationLabel;
        if (state.LabelIsChildOfUpgrade)
        {
            orientationLabel = tickbox.GetNodeOrNull<MegaLabel>("ShowUpgradeLabel")
                ?? FindMegaLabel(tickbox);
            if (orientationLabel != null)
            {
                orientationLabel.Name = StandaloneLabelName;
                orientationLabel.UniqueNameInOwner = false;
            }
        }
        else
        {
            orientationLabel = (MegaLabel)label.Duplicate((int)Node.DuplicateFlags.Scripts);
            orientationLabel.Name = StandaloneLabelName;
            orientationLabel.UniqueNameInOwner = false;
            parent.AddChild(orientationLabel);
            orientationLabel.Visible = false;
            state.StandaloneLabel = orientationLabel;
        }

        orientationLabel?.SetTextAutoSize(
            new LocString("card_selection", "FEIYAP_VIEW_ORIENTATIONS").GetFormattedText());

        tickbox.IsTicked = false;
        tickbox.Visible = false;
        tickbox.Disable();
        tickbox.Connect(NTickbox.SignalName.Toggled, Callable.From<NTickbox>(_ => OnOrientationToggled(screen)));

        state.Tickbox = tickbox;
        state.Created = true;
        Layout(screen, state);
    }

    public static void OnOpen(NInspectCardScreen screen)
    {
        EnsureCreated(screen);
        if (!States.TryGetValue(screen, out var state) || !state.Created)
        {
            return;
        }

        state.Tickbox.Enable();
        SyncForCurrentCard(screen, state);
    }

    public static void OnClose(NInspectCardScreen screen)
    {
        if (!States.TryGetValue(screen, out var state) || !state.Created)
        {
            return;
        }

        state.Tickbox.Disable();
    }

    public static void OnSetCard(NInspectCardScreen screen)
    {
        EnsureCreated(screen);
        if (!States.TryGetValue(screen, out var state) || !state.Created)
        {
            return;
        }

        SyncForCurrentCard(screen, state);
        ApplyPortraitOverride(screen);
    }

    public static void ApplyPortraitOverride(NInspectCardScreen screen)
    {
        if (!States.TryGetValue(screen, out var state) || !state.Created)
        {
            return;
        }

        if (CardField?.GetValue(screen) is not NCard cardNode
            || cardNode.Model is not FeiyapTarotCardBase tarot)
        {
            return;
        }

        if (!state.Tickbox.Visible)
        {
            tarot.PortraitOrientationOverride = null;
            tarot.ApplyPortraitToNode(cardNode);
            return;
        }

        tarot.PortraitOrientationOverride = state.Tickbox.IsTicked;
        tarot.ApplyPortraitToNode(cardNode);
    }

    private static void OnOrientationToggled(NInspectCardScreen screen) =>
        ApplyPortraitOverride(screen);

    private static void SyncForCurrentCard(NInspectCardScreen screen, State state)
    {
        var card = GetCurrentCard(screen);
        var show = card is FeiyapTarotCardBase tarot && tarot.HasInspectableReversedPortrait;
        state.Tickbox.Visible = show;
        state.Tickbox.MouseFilter = show
            ? Control.MouseFilterEnum.Stop
            : Control.MouseFilterEnum.Ignore;

        if (state.StandaloneLabel != null)
        {
            state.StandaloneLabel.Visible = show;
        }

        if (show && card is FeiyapTarotCardBase currentTarot)
        {
            // 未勾选=正位，勾选=逆位；切入时跟随当前展示朝向
            state.Tickbox.IsTicked = currentTarot.DisplaysReversedPortrait;
        }
        else
        {
            state.Tickbox.IsTicked = false;
        }

        Layout(screen, state);
    }

    private static void Layout(NInspectCardScreen screen, State state)
    {
        var upgrade = UpgradeTickboxField?.GetValue(screen) as NTickbox
            ?? screen.GetNodeOrNull<NTickbox>("%Upgrade");
        if (upgrade == null)
        {
            return;
        }

        var upgradeVisible = upgrade.Visible;
        var orientationVisible = state.Tickbox.Visible;

        // 恢复升级勾选框基准位置，再按是否并排展示偏移
        upgrade.Position = state.UpgradeBasePosition;
        var upgradeLabel = screen.GetNodeOrNull<MegaLabel>("%ShowUpgradeLabel");
        if (upgradeLabel != null && !state.LabelIsChildOfUpgrade)
        {
            upgradeLabel.Position = state.LabelBasePosition;
        }

        if (!orientationVisible)
        {
            return;
        }

        if (upgradeVisible)
        {
            var pairWidth = Math.Max(
                MeasurePairWidth(upgrade, upgradeLabel, state.LabelIsChildOfUpgrade),
                160f);
            var shift = (pairWidth + HorizontalGap) * 0.5f;
            upgrade.Position = state.UpgradeBasePosition - new Vector2(shift, 0f);
            if (upgradeLabel != null && !state.LabelIsChildOfUpgrade)
            {
                upgradeLabel.Position = state.LabelBasePosition - new Vector2(shift, 0f);
            }

            state.Tickbox.Position = upgrade.Position + new Vector2(pairWidth + HorizontalGap, 0f);
            if (state.StandaloneLabel != null)
            {
                state.StandaloneLabel.Position = state.Tickbox.Position + state.LabelOffsetFromUpgrade;
            }
        }
        else
        {
            // 无升级选项时，正逆位勾选框占据原升级位置
            state.Tickbox.Position = state.UpgradeBasePosition;
            if (state.StandaloneLabel != null)
            {
                state.StandaloneLabel.Position = state.LabelBasePosition;
            }
        }
    }

    private static float MeasurePairWidth(NTickbox upgrade, MegaLabel? label, bool labelIsChild)
    {
        var width = upgrade.Size.X;
        if (label == null)
        {
            return Math.Max(width, 1f);
        }

        if (labelIsChild)
        {
            return Math.Max(width, label.Position.X + label.Size.X);
        }

        var left = Math.Min(upgrade.Position.X, label.Position.X);
        var right = Math.Max(upgrade.Position.X + upgrade.Size.X, label.Position.X + label.Size.X);
        return Math.Max(right - left, 1f);
    }

    private static CardModel? GetCurrentCard(NInspectCardScreen screen)
    {
        if (CardsField?.GetValue(screen) is not List<CardModel> cards
            || IndexField?.GetValue(screen) is not int index
            || index < 0
            || index >= cards.Count)
        {
            return null;
        }

        return cards[index];
    }

    private static MegaLabel? FindMegaLabel(Node root)
    {
        foreach (var child in root.GetChildren())
        {
            if (child is MegaLabel label)
            {
                return label;
            }

            var nested = FindMegaLabel(child);
            if (nested != null)
            {
                return nested;
            }
        }

        return null;
    }

    /// <summary>
    /// 将副本子树的 Owner 设为副本根，使 %UniqueName 在本地解析。
    /// </summary>
    private static void ReownDescendants(Node root, Node owner)
    {
        foreach (var child in root.GetChildren())
        {
            child.Owner = owner;
            ReownDescendants(child, owner);
        }
    }

    private static void DisconnectAll(GodotObject source, StringName signal)
    {
        foreach (var connection in source.GetSignalConnectionList(signal))
        {
            var callable = connection["callable"].AsCallable();
            if (source.IsConnected(signal, callable))
            {
                source.Disconnect(signal, callable);
            }
        }
    }
}
