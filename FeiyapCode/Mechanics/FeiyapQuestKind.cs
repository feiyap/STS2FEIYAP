namespace Feiyap.Mechanics;

/// <summary>
/// 转职任务种类，用于追踪单局内完成的任务数量。
/// </summary>
public enum FeiyapQuestKind
{
    MiWang = 1,
    KuXiu = 2,
    LiuLang = 4,
    FaWei = 8
}

internal static class FeiyapQuestKindExtensions
{
    internal const int AllQuestMask =
        (int)FeiyapQuestKind.MiWang |
        (int)FeiyapQuestKind.KuXiu |
        (int)FeiyapQuestKind.LiuLang |
        (int)FeiyapQuestKind.FaWei;
}
