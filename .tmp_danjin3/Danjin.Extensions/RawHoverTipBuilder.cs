using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using Godot;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Logging;

namespace Danjin.Extensions;

internal static class RawHoverTipBuilder
{
	private static FieldInfo? _titleField;

	private static FieldInfo? _descField;

	private static FieldInfo? _idField;

	private static bool _reflectionAttempted;

	private static readonly Dictionary<string, string> _engStrict = new Dictionary<string, string>
	{
		["攻势1"] = "ATK Stance 1",
		["攻势2"] = "ATK Stance 2",
		["守势1"] = "DEF Stance 1",
		["守势4"] = "DEF Stance 4",
		["重放"] = "Replay",
		["生命值消耗"] = "HP Cost",
		["打出时需要至少[blue]1[/blue]层[red]攻势[/red]，否则该效果不生效。"] = "Requires at least [blue]1[/blue] [red]ATK Stance[/red] to take effect.",
		["打出时需要至少[blue]2[/blue]层[red]攻势[/red]，否则该效果不生效。"] = "Requires at least [blue]2[/blue] [red]ATK Stance[/red] to take effect.",
		["打出时需要至少[blue]2[/blue]层[red]攻势[/red]，否则该效果不会生效。"] = "Requires at least [blue]2[/blue] [red]ATK Stance[/red] to take effect.",
		["打出时需要至少[blue]1[/blue]层[blue]守势[/blue]，否则该效果不生效。"] = "Requires at least [blue]1[/blue] [blue]DEF Stance[/blue] to take effect.",
		["打出时需要至少[blue]4[/blue]层[blue]守势[/blue]，否则该效果不生效。"] = "Requires at least [blue]4[/blue] [blue]DEF Stance[/blue] to take effect.",
		["这张牌打出后会额外再打出一次。"] = "This card is played one additional time after being played.",
		["失去当前生命值的[red]10%[/red](最少1点)。"] = "Lose [red]10%[/red] of your current HP (minimum 1)."
	};

	private static readonly (Regex, string)[] _engPatterns = new(Regex, string)[3]
	{
		(new Regex("^失去\\[red\\](\\d+)\\[/red\\]点生命值。$"), "Lose [red]$1[/red] HP."),
		(new Regex("^失去最大生命值的\\[red\\](\\d+)%\\[/red\\]\\(不致死\\)。$"), "Lose [red]$1%[/red] of max HP (cannot be lethal)."),
		(new Regex("^失去当前生命值的\\[red\\](\\d+)%\\[/red\\]\\(最少1点\\)。$"), "Lose [red]$1%[/red] of current HP (minimum 1).")
	};

	private static readonly Dictionary<string, string> _korStrict = new Dictionary<string, string>
	{
		["攻势1"] = "공세 1",
		["攻势2"] = "공세 2",
		["守势1"] = "수세 1",
		["守势4"] = "수세 4",
		["重放"] = "재사용",
		["生命值消耗"] = "체력 소모",
		["打出时需要至少[blue]1[/blue]层[red]攻势[/red]，否则该效果不生效。"] = "사용 시 최소 [blue]1[/blue]의 [red]공세[/red]가 필요합니다. 그렇지 않으면 효과가 발동하지 않습니다.",
		["打出时需要至少[blue]2[/blue]层[red]攻势[/red]，否则该效果不生效。"] = "사용 시 최소 [blue]2[/blue]의 [red]공세[/red]가 필요합니다. 그렇지 않으면 효과가 발동하지 않습니다.",
		["打出时需要至少[blue]2[/blue]层[red]攻势[/red]，否则该效果不会生效。"] = "사용 시 최소 [blue]2[/blue]의 [red]공세[/red]가 필요합니다. 그렇지 않으면 효과가 발동하지 않습니다.",
		["打出时需要至少[blue]1[/blue]层[blue]守势[/blue]，否则该效果不生效。"] = "사용 시 최소 [blue]1[/blue]의 [blue]수세[/blue]가 필요합니다. 그렇지 않으면 효과가 발동하지 않습니다.",
		["打出时需要至少[blue]4[/blue]层[blue]守势[/blue]，否则该效果不生效。"] = "사용 시 최소 [blue]4[/blue]의 [blue]수세[/blue]가 필요합니다. 그렇지 않으면 효과가 발동하지 않습니다.",
		["这张牌打出后会额外再打出一次。"] = "이 카드는 사용 후 한 번 더 발동합니다.",
		["失去当前生命值的[red]10%[/red](最少1点)。"] = "현재 체력의 [red]10%[/red]를 잃습니다 (최소 1)."
	};

	private static readonly (Regex, string)[] _korPatterns = new(Regex, string)[3]
	{
		(new Regex("^失去\\[red\\](\\d+)\\[/red\\]点生命值。$"), "[red]$1[/red]의 체력을 잃습니다."),
		(new Regex("^失去最大生命值的\\[red\\](\\d+)%\\[/red\\]\\(不致死\\)。$"), "최대 체력의 [red]$1%[/red]를 잃습니다 (죽지 않음)."),
		(new Regex("^失去当前生命值的\\[red\\](\\d+)%\\[/red\\]\\(最少1点\\)。$"), "현재 체력의 [red]$1%[/red]를 잃습니다 (최소 1).")
	};

	private static readonly Dictionary<string, string> _jpnStrict = new Dictionary<string, string>
	{
		["攻势1"] = "攻勢 1",
		["攻势2"] = "攻勢 2",
		["守势1"] = "守勢 1",
		["守势4"] = "守勢 4",
		["重放"] = "リプレイ",
		["生命值消耗"] = "HPコスト",
		["打出时需要至少[blue]1[/blue]层[red]攻势[/red]，否则该效果不生效。"] = "プレイするには[red]攻勢[/red]が[blue]1[/blue]以上必要。効果なし。",
		["打出时需要至少[blue]2[/blue]层[red]攻势[/red]，否则该效果不生效。"] = "プレイするには[red]攻勢[/red]が[blue]2[/blue]以上必要。効果なし。",
		["打出时需要至少[blue]2[/blue]层[red]攻势[/red]，否则该效果不会生效。"] = "プレイするには[red]攻勢[/red]が[blue]2[/blue]以上必要。効果なし。",
		["打出时需要至少[blue]1[/blue]层[blue]守势[/blue]，否则该效果不生效。"] = "プレイするには[blue]守勢[/blue]が[blue]1[/blue]以上必要。効果なし。",
		["打出时需要至少[blue]4[/blue]层[blue]守势[/blue]，否则该效果不生效。"] = "プレイするには[blue]守勢[/blue]が[blue]4[/blue]以上必要。効果なし。",
		["这张牌打出后会额外再打出一次。"] = "このカードはプレイ後にもう1回プレイされる。",
		["失去当前生命值的[red]10%[/red](最少1点)。"] = "現在HPの[red]10%[/red]を失う(最少1)。"
	};

	private static readonly (Regex, string)[] _jpnPatterns = new(Regex, string)[3]
	{
		(new Regex("^失去\\[red\\](\\d+)\\[/red\\]点生命值。$"), "[red]$1[/red]のHPを失う。"),
		(new Regex("^失去最大生命值的\\[red\\](\\d+)%\\[/red\\]\\(不致死\\)。$"), "最大HPの[red]$1%[/red]を失う(致死しない)。"),
		(new Regex("^失去当前生命值的\\[red\\](\\d+)%\\[/red\\]\\(最少1点\\)。$"), "現在HPの[red]$1%[/red]を失う(最少1)。")
	};

	private static void TryInitReflection()
	{
		if (!_reflectionAttempted)
		{
			_reflectionAttempted = true;
			_titleField = typeof(HoverTip).GetField("<Title>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);
			_descField = typeof(HoverTip).GetField("<Description>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);
			_idField = typeof(HoverTip).GetField("<Id>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);
		}
	}

	public static IHoverTip Build(string id, string title, string description)
	{
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Expected O, but got Unknown
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		title = Localize(title);
		description = Localize(description);
		TryInitReflection();
		try
		{
			object obj = (object)default(HoverTip);
			if (_titleField != null && _descField != null && _idField != null)
			{
				_titleField.SetValue(obj, title);
				_descField.SetValue(obj, description);
				_idField.SetValue(obj, id);
				return (IHoverTip)(object)(HoverTip)obj;
			}
			return (IHoverTip)(object)new HoverTip(new LocString("card_keywords", "DANJIN_KEYWORD_FEIREN.title"), description, (Texture2D)null);
		}
		catch (Exception value)
		{
			Log.Error($"[DanjinMod] RawHoverTipBuilder.Build 反射失败: {value}", 1);
			return (IHoverTip)(object)default(HoverTip);
		}
	}

	private static string Localize(string text)
	{
		if (string.IsNullOrEmpty(text))
		{
			return text;
		}
		LocManager instance = LocManager.Instance;
		string text2 = ((instance != null) ? instance.Language : null);
		Dictionary<string, string> dictionary = null;
		(Regex, string)[] array = null;
		switch (text2)
		{
		case "eng":
			dictionary = _engStrict;
			array = _engPatterns;
			break;
		case "kor":
			dictionary = _korStrict;
			array = _korPatterns;
			break;
		case "jpn":
			dictionary = _jpnStrict;
			array = _jpnPatterns;
			break;
		}
		if (dictionary == null || array == null)
		{
			return text;
		}
		if (dictionary.TryGetValue(text, out var value))
		{
			return value;
		}
		(Regex, string)[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			var (regex, replacement) = array2[i];
			if (regex.Match(text).Success)
			{
				return regex.Replace(text, replacement);
			}
		}
		return text;
	}
}
