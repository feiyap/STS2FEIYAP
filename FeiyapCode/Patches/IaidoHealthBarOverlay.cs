using System.Reflection;
using System.Runtime.CompilerServices;
using Feiyap.Characters;
using Feiyap.Powers;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.addons.mega_text;
using STS2RitsuLib.Scaffolding.Characters;

namespace Feiyap.Patches;

/// <summary>
/// 将居合数值显示在格挡 UI 槽位。
/// </summary>
internal static class IaidoHealthBarOverlay
{
    private enum DisplayMode
    {
        None,
        Block,
        Iaido
    }

    private sealed class State
    {
        public int LastIaido;
        public int LastBlock;
        public bool CycleRunning;
        public bool PreferIaido = true;
        public DisplayMode LastMode = DisplayMode.None;
        public bool IsFading;
        public int FadeSerial;
        public bool HasOriginalBlockLabelPosition;
        public Vector2 OriginalBlockLabelPosition;
        public readonly Dictionary<CanvasItem, bool> OriginalBlockGraphicVisibility = [];
    }

    private static readonly ConditionalWeakTable<NHealthBar, State> States = new();

    private static readonly FieldInfo? CreatureField = AccessTools.Field(typeof(NHealthBar), "_creature");

    private static readonly FieldInfo? OriginalBlockPositionField =
        AccessTools.Field(typeof(NHealthBar), "_originalBlockPosition");

    private const string IaidoIconPath = $"{Entry.ResPath}/images/ui/iaido_block_icon.png";

    private const string IaidoIconName = "FeiyapIaidoIcon";

    private static Texture2D? _iaidoIconTexture;

    private static readonly Color IaidoCyan = new("6EBFD4");

    private static readonly Color IaidoCyanDark = new("3A7A8C");

    private static readonly Color IaidoHpForeground = new("6EBFD4");

    private static readonly Color VanillaBlockOutline = new("1B3045");

    private static readonly Color VanillaHpForeground = new("F1373E");

    private static readonly Color VanillaBlockHpForeground = new("3B6FA3");

    private static readonly Color White = Colors.White;

    private const double CycleSeconds = 0.72;

    private const float IaidoLabelOffsetX = -3f;

    private const double FadeSeconds = 0.16;

    private const int FadeSteps = 4;

    public static Creature? GetCreature(NHealthBar healthBar)
    {
        var value = CreatureField?.GetValue(healthBar);
        return value as Creature;
    }

    public static bool IsFeiyapPlayer(Creature creature) =>
        creature.Player?.Character is FeiyapCharacter;

    public static void RefreshForCreature(Creature? creature)
    {
        if (creature == null || !IsFeiyapPlayer(creature))
        {
            return;
        }

        if (Engine.GetMainLoop() is not SceneTree tree || tree.Root == null)
        {
            return;
        }

        foreach (var healthBar in EnumerateHealthBars(tree.Root))
        {
            if (GetCreature(healthBar) == creature)
            {
                Apply(healthBar);
                return;
            }
        }

        var creatureNode = NCombatRoom.Instance?.GetCreatureNode(creature);
        if (creatureNode == null)
        {
            return;
        }

        foreach (var healthBar in EnumerateHealthBars(creatureNode))
        {
            if (GetCreature(healthBar) == creature)
            {
                Apply(healthBar);
                break;
            }
        }
    }

    public static void Apply(NHealthBar healthBar)
    {
        var creature = GetCreature(healthBar);
        if (creature == null || !IsFeiyapPlayer(creature))
        {
            return;
        }

        var iaido = creature.FindPower<FeiyapIaidoPower>()?.Amount ?? 0;
        var block = Math.Max(0, creature.Block);
        var state = States.GetOrCreateValue(healthBar);
        state.LastIaido = iaido;
        state.LastBlock = block;

        if (block > 0 && iaido > 0)
        {
            StartAlternatingCycle(healthBar, state);
            var mode = state.LastMode is DisplayMode.Block or DisplayMode.Iaido
                ? state.LastMode
                : state.PreferIaido ? DisplayMode.Iaido : DisplayMode.Block;

            if (!state.IsFading)
            {
                ApplyMode(healthBar, mode, block, iaido, state);
            }
        }
        else
        {
            state.PreferIaido = true;
            state.FadeSerial++;
            state.IsFading = false;

            if (iaido > 0)
            {
                ApplyMode(healthBar, DisplayMode.Iaido, block, iaido, state);
            }
            else
            {
                ApplyMode(healthBar, block > 0 ? DisplayMode.Block : DisplayMode.None, block, iaido, state);
            }
        }
    }

    private static void StartAlternatingCycle(NHealthBar healthBar, State state)
    {
        if (state.CycleRunning)
        {
            return;
        }

        state.CycleRunning = true;
        _ = RunAlternatingCycleAsync(healthBar, state);
    }

    private static async Task RunAlternatingCycleAsync(NHealthBar healthBar, State state)
    {
        try
        {
            while (GodotObject.IsInstanceValid(healthBar))
            {
                var tree = healthBar.GetTree();
                if (tree == null)
                {
                    break;
                }

                var timer = tree.CreateTimer(CycleSeconds, true, false, false);
                await healthBar.ToSignal(timer, SceneTreeTimer.SignalName.Timeout);

                var creature = GetCreature(healthBar);
                if (creature == null)
                {
                    break;
                }

                var iaido = creature.FindPower<FeiyapIaidoPower>()?.Amount ?? 0;
                var block = Math.Max(0, creature.Block);
                if (iaido <= 0 || block <= 0)
                {
                    break;
                }

                state.PreferIaido = !state.PreferIaido;
                state.LastIaido = iaido;
                state.LastBlock = block;
                var mode = state.PreferIaido ? DisplayMode.Iaido : DisplayMode.Block;
                await FadeToModeAsync(healthBar, mode, block, iaido, state);
            }
        }
        catch (Exception ex)
        {
            Entry.Logger.Warn($"[IaidoHealthBar] 交替显示停止: {ex.Message}");
        }
        finally
        {
            state.CycleRunning = false;
            state.IsFading = false;
            if (GodotObject.IsInstanceValid(healthBar))
            {
                Apply(healthBar);
            }
        }
    }

    private static async Task FadeToModeAsync(
        NHealthBar healthBar,
        DisplayMode mode,
        int block,
        int iaido,
        State state)
    {
        var blockContainer = healthBar.GetNodeOrNull<Control>("%BlockContainer");
        if (blockContainer == null)
        {
            ApplyMode(healthBar, mode, block, iaido, state);
            return;
        }

        var serial = ++state.FadeSerial;
        state.IsFading = true;

        try
        {
            await FadeContainerAlphaAsync(healthBar, blockContainer, 1f, 0.18f, serial, state);
            if (serial == state.FadeSerial && GodotObject.IsInstanceValid(healthBar))
            {
                ApplyMode(healthBar, mode, block, iaido, state);
                SetContainerAlpha(blockContainer, 0.18f);
                await FadeContainerAlphaAsync(healthBar, blockContainer, 0.18f, 1f, serial, state);
            }
        }
        finally
        {
            if (serial == state.FadeSerial)
            {
                state.IsFading = false;
            }

            if (GodotObject.IsInstanceValid(blockContainer))
            {
                SetContainerAlpha(blockContainer, 1f);
            }
        }
    }

    private static async Task FadeContainerAlphaAsync(
        NHealthBar healthBar,
        Control blockContainer,
        float from,
        float to,
        int serial,
        State state)
    {
        var tree = healthBar.GetTree();
        if (tree == null)
        {
            SetContainerAlpha(blockContainer, to);
            return;
        }

        for (var i = 1; i <= FadeSteps; i++)
        {
            if (serial != state.FadeSerial
                || !GodotObject.IsInstanceValid(healthBar)
                || !GodotObject.IsInstanceValid(blockContainer))
            {
                return;
            }

            var t = (float)i / FadeSteps;
            SetContainerAlpha(blockContainer, from + (to - from) * t);
            var timer = tree.CreateTimer(FadeSeconds / FadeSteps, true, false, false);
            await healthBar.ToSignal(timer, SceneTreeTimer.SignalName.Timeout);
        }

        if (serial == state.FadeSerial && GodotObject.IsInstanceValid(blockContainer))
        {
            SetContainerAlpha(blockContainer, to);
        }
    }

    private static void SetContainerAlpha(Control blockContainer, float alpha)
    {
        var modulate = blockContainer.Modulate;
        modulate.A = alpha;
        blockContainer.Modulate = modulate;
    }

    private static void ApplyMode(
        NHealthBar healthBar,
        DisplayMode mode,
        int block,
        int iaido,
        State state)
    {
        var blockContainer = healthBar.GetNodeOrNull<Control>("%BlockContainer");
        var blockLabel = healthBar.GetNodeOrNull<MegaLabel>("%BlockLabel");
        var blockOutline = healthBar.GetNodeOrNull<Control>("%BlockOutline");
        var hpForeground = healthBar.GetNodeOrNull<Control>("%HpForeground");

        if (blockContainer == null || blockLabel == null)
        {
            return;
        }

        blockContainer.Modulate = White;
        if (OriginalBlockPositionField?.GetValue(healthBar) is Vector2 position)
        {
            blockContainer.Position = position;
        }

        switch (mode)
        {
            case DisplayMode.Iaido:
                blockContainer.Visible = true;
                if (blockOutline != null)
                {
                    blockOutline.Visible = true;
                    blockOutline.SelfModulate = IaidoCyanDark;
                }

                if (hpForeground != null)
                {
                    hpForeground.SelfModulate = IaidoHpForeground;
                }

                blockLabel.SetTextAutoSize(iaido.ToString());
                ApplyBlockLabelPosition(blockLabel, state, iaidoMode: true);
                blockLabel.SelfModulate = White;
                blockLabel.Modulate = White;
                TryTintBlockGraphics(blockContainer, IaidoCyan, White);
                ApplyIaidoIcon(blockContainer, state, visible: true);
                state.LastMode = DisplayMode.Iaido;
                break;

            case DisplayMode.Block:
                if (block <= 0)
                {
                    ApplyMode(healthBar, DisplayMode.None, block, iaido, state);
                    break;
                }

                blockContainer.Visible = true;
                if (blockOutline != null)
                {
                    blockOutline.Visible = true;
                    blockOutline.SelfModulate = VanillaBlockOutline;
                }

                if (hpForeground != null)
                {
                    hpForeground.SelfModulate = VanillaBlockHpForeground;
                }

                blockLabel.SetTextAutoSize(block.ToString());
                ApplyBlockLabelPosition(blockLabel, state, iaidoMode: false);
                blockLabel.SelfModulate = White;
                blockLabel.Modulate = White;
                ApplyIaidoIcon(blockContainer, state, visible: false);
                ResetBlockGraphics(blockContainer);
                state.LastMode = DisplayMode.Block;
                break;

            default:
                blockContainer.Visible = false;
                if (blockOutline != null)
                {
                    blockOutline.Visible = false;
                    blockOutline.SelfModulate = VanillaBlockOutline;
                }

                if (hpForeground != null)
                {
                    hpForeground.SelfModulate = VanillaHpForeground;
                }

                ApplyIaidoIcon(blockContainer, state, visible: false);
                ApplyBlockLabelPosition(blockLabel, state, iaidoMode: false);
                ResetBlockGraphics(blockContainer);
                state.LastMode = DisplayMode.None;
                break;
        }
    }

    private static void ApplyBlockLabelPosition(MegaLabel blockLabel, State state, bool iaidoMode)
    {
        if (!state.HasOriginalBlockLabelPosition)
        {
            state.OriginalBlockLabelPosition = blockLabel.Position;
            state.HasOriginalBlockLabelPosition = true;
        }

        blockLabel.Position = state.OriginalBlockLabelPosition
                              + (iaidoMode ? new Vector2(IaidoLabelOffsetX, 0f) : Vector2.Zero);
    }

    private static void ApplyIaidoIcon(Control blockContainer, State state, bool visible)
    {
        var icon = EnsureIaidoIcon(blockContainer);
        if (icon == null)
        {
            if (!visible)
            {
                SetOriginalBlockGraphicsHidden(blockContainer, state, hidden: false);
            }

            return;
        }

        SetOriginalBlockGraphicsHidden(blockContainer, state, visible);
        icon.Visible = visible;
        icon.Modulate = White;
        icon.SelfModulate = White;
    }

    private static TextureRect? EnsureIaidoIcon(Control blockContainer)
    {
        var existing = blockContainer.FindChild(IaidoIconName, false, false) as TextureRect;
        if (existing != null)
        {
            return existing;
        }

        var texture = GetIaidoIconTexture();
        if (texture == null)
        {
            return null;
        }

        var icon = new TextureRect
        {
            Name = IaidoIconName,
            Texture = texture,
            Visible = false,
            MouseFilter = Control.MouseFilterEnum.Ignore,
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
            StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered
        };
        icon.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        blockContainer.AddChild(icon);
        blockContainer.MoveChild(icon, 0);
        return icon;
    }

    private static Texture2D? GetIaidoIconTexture()
    {
        if (_iaidoIconTexture != null)
        {
            return _iaidoIconTexture;
        }

        _iaidoIconTexture = ResourceLoader.Load<Texture2D>(IaidoIconPath);
        return _iaidoIconTexture;
    }

    private static void SetOriginalBlockGraphicsHidden(Node node, State state, bool hidden)
    {
        SetOriginalBlockGraphicsHiddenRecursive(node, state, hidden);
        if (!hidden)
        {
            state.OriginalBlockGraphicVisibility.Clear();
        }
    }

    private static void SetOriginalBlockGraphicsHiddenRecursive(Node node, State state, bool hidden)
    {
        foreach (var child in node.GetChildren(false))
        {
            if (child.Name == IaidoIconName)
            {
                continue;
            }

            if (child is CanvasItem canvas && !IsTextNode(child) && !ContainsTextNode(child))
            {
                if (hidden)
                {
                    state.OriginalBlockGraphicVisibility.TryAdd(canvas, canvas.Visible);
                    canvas.Visible = false;
                }
                else if (state.OriginalBlockGraphicVisibility.TryGetValue(canvas, out var wasVisible))
                {
                    canvas.Visible = wasVisible;
                }
            }

            SetOriginalBlockGraphicsHiddenRecursive(child, state, hidden);
        }
    }

    private static bool ContainsTextNode(Node node)
    {
        foreach (var child in node.GetChildren(false))
        {
            if (IsTextNode(child) || ContainsTextNode(child))
            {
                return true;
            }
        }

        return false;
    }

    private static IEnumerable<NHealthBar> EnumerateHealthBars(Node root)
    {
        if (root is NHealthBar healthBar)
        {
            yield return healthBar;
        }

        foreach (var child in root.GetChildren(false))
        {
            foreach (var nested in EnumerateHealthBars(child))
            {
                yield return nested;
            }
        }
    }

    private static void TryTintBlockGraphics(Node node, Color graphicColor, Color textColor)
    {
        foreach (var child in node.GetChildren(false))
        {
            if (child is CanvasItem canvas)
            {
                canvas.SelfModulate = IsTextNode(child) ? textColor : graphicColor;
            }

            TryTintBlockGraphics(child, graphicColor, textColor);
        }
    }

    private static void ResetBlockGraphics(Node node)
    {
        foreach (var child in node.GetChildren(false))
        {
            if (child is CanvasItem canvas)
            {
                canvas.SelfModulate = White;
            }

            ResetBlockGraphics(child);
        }
    }

    private static bool IsTextNode(Node node)
    {
        var typeName = node.GetType().Name;
        var nodeName = node.Name.ToString();
        return node is Label
               || node is MegaLabel
               || typeName.Contains("Label", StringComparison.OrdinalIgnoreCase)
               || nodeName.Contains("Label", StringComparison.OrdinalIgnoreCase)
               || nodeName.Contains("Text", StringComparison.OrdinalIgnoreCase);
    }
}
