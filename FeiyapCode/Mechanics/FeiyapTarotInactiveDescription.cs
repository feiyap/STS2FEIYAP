using System.Text.RegularExpressions;
using Godot;
using MegaCrit.Sts2.Core.Helpers;
using STS2RitsuLib.Keywords;

namespace Feiyap.Mechanics;

/// <summary>
/// 将塔罗牌描述中当前不会生效的正/逆位行置灰，便于对比哪条效果生效。
/// </summary>
public static class FeiyapTarotInactiveDescription
{
    /// <summary>原版自定义色标会覆盖 <c>[color]</c>，置灰前需剥掉。</summary>
    private static readonly Regex ColorFxTagRegex = new(
        @"\[/?(?:gold|green|red|blue|purple|orange|pink|aqua)\]",
        RegexOptions.CultureInvariant);

    private static readonly string GrayPrefix = $"[color=#{StsColors.gray.ToHtml(false)}]";
    private const string GraySuffix = "[/color]";

    /// <summary>
    /// 按正/逆位是否生效，把对应描述行包进灰色 BBCode。
    /// </summary>
    public static string Apply(string description, bool uprightActive, bool reversedActive)
    {
        if (string.IsNullOrEmpty(description) || (uprightActive && reversedActive))
        {
            return description;
        }

        var uprightTitle = GetKeywordTitle(FeiyapKeywords.TarotUprightId);
        var reversedTitle = GetKeywordTitle(FeiyapKeywords.TarotReversedId);
        if (string.IsNullOrEmpty(uprightTitle) || string.IsNullOrEmpty(reversedTitle))
        {
            return description;
        }

        var lines = description.Split('\n');
        var changed = false;
        for (var i = 0; i < lines.Length; i++)
        {
            var line = lines[i];
            if (string.IsNullOrEmpty(line) || line.StartsWith(GrayPrefix, StringComparison.Ordinal))
            {
                continue;
            }

            var hasUpright = line.Contains(uprightTitle, StringComparison.Ordinal);
            var hasReversed = line.Contains(reversedTitle, StringComparison.Ordinal);
            if (hasUpright == hasReversed)
            {
                continue;
            }

            var inactive = (hasUpright && !uprightActive) || (hasReversed && !reversedActive);
            if (!inactive)
            {
                continue;
            }

            lines[i] = WrapGray(line);
            changed = true;
        }

        return changed ? string.Join('\n', lines) : description;
    }

    private static string GetKeywordTitle(string keywordId)
    {
        try
        {
            return ModKeywordRegistry.GetTitle(keywordId).GetFormattedText() ?? string.Empty;
        }
        catch (Exception)
        {
            return string.Empty;
        }
    }

    private static string WrapGray(string line)
    {
        var stripped = ColorFxTagRegex.Replace(line, string.Empty);
        return string.IsNullOrEmpty(stripped) ? line : GrayPrefix + stripped + GraySuffix;
    }
}
