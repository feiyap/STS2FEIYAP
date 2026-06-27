namespace Feiyap.Mechanics;

/// <summary>
/// 标记当前是否正在计算敌人攻击意图的预览伤害。
/// </summary>
internal static class FeiyapIaidoIntentPreviewScope
{
    private static int _depth;

    internal static bool IsActive => _depth > 0;

    internal static void Enter() => _depth++;

    internal static void Exit() => _depth = Math.Max(0, _depth - 1);
}
