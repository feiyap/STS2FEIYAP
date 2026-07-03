using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Danjin.Patches;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib;
using STS2RitsuLib.Keywords;

namespace Danjin.Cards;

public class DanjinCardKeywords
{
	[HarmonyPatch(typeof(ModelDb), "Init")]
	private class InjectKeywordLocPatch
	{
		[CompilerGenerated]
		private static class _003C_003EO
		{
			public static LocaleChangeCallback _003C0_003E__InjectForCurrentLanguage;
		}

		private static bool _subscribed;

		[HarmonyPostfix]
		private static void Inject()
		{
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0033: Expected O, but got Unknown
			InjectForCurrentLanguage();
			if (!_subscribed && LocManager.Instance != null)
			{
				LocManager instance = LocManager.Instance;
				object obj = _003C_003EO._003C0_003E__InjectForCurrentLanguage;
				if (obj == null)
				{
					LocaleChangeCallback val = InjectForCurrentLanguage;
					_003C_003EO._003C0_003E__InjectForCurrentLanguage = val;
					obj = (object)val;
				}
				instance.SubscribeToLocaleChange((LocaleChangeCallback)obj);
				_subscribed = true;
			}
			FeirenHealthForecast.Register();
			ChuXueHealthForecast.Register();
		}

		private static void InjectForCurrentLanguage()
		{
			LocManager instance = LocManager.Instance;
			LocTable val = ((instance != null) ? instance.GetTable("card_keywords") : null);
			if (val != null && AccessTools.Field(typeof(LocTable), "_translations").GetValue(val) is Dictionary<string, string> dictionary)
			{
				string language = LocManager.Instance.Language;
				Log.Info("[Danjin] InjectKeywordLocPatch: re-injecting for language '" + language + "'", 1);
				switch (language)
				{
				case "eng":
					dictionary["DANJIN_KEYWORD_FEIREN.title"] = "Crimson Blade";
					dictionary["DANJIN_KEYWORD_FEIREN.description"] = "Each attack grants 1 [color=#E84393]Ruby Blossom[/color] and costs [blue]3%[/blue] of max HP (cannot be lethal). When HP is insufficient, you still gain [color=#E84393]Ruby Blossom[/color] but the card's values are halved.";
					dictionary["DANJIN_KEYWORD_LIAOYU.title"] = "Mending";
					dictionary["DANJIN_KEYWORD_LIAOYU.description"] = "Each [color=#E84393]Ruby Blossom[/color] spent restores [blue]3%[/blue] of max HP. Total HP recovered via [color=#E84393]Ruby Blossom[/color] this combat cannot exceed the total HP spent.";
					dictionary["DANJIN_KEYWORD_LIUZHUAN.title"] = "Flux";
					dictionary["DANJIN_KEYWORD_LIUZHUAN.description"] = "Switch to the other [gold]Stance[/gold] ([red]ATK Stance[/red] ↔ [blue]DEF Stance[/blue]), preserving the current stack count.";
					dictionary["DANJIN_KEYWORD_CANXIN.title"] = "Zanshin";
					dictionary["DANJIN_KEYWORD_CANXIN.description"] = "Keep the current [gold]Stance[/gold] and increase its stacks by [blue]1[/blue].";
					dictionary["DANJIN_KEYWORD_LIANDUAN.title"] = "Combo";
					dictionary["DANJIN_KEYWORD_LIANDUAN.description"] = "[gold]Ethereal[/gold], [gold]Exhaust[/gold]. After you play any other card, this card is [gold]Exhausted[/gold].";
					dictionary["DANJIN_KEYWORD_KUJIE.title"] = "Withered";
					dictionary["DANJIN_KEYWORD_KUJIE.description"] = "This combat, [gold]Incinerating Will[/gold] no longer takes effect and you can no longer apply [gold]Incinerating Will[/gold].";
					break;
				case "kor":
					dictionary["DANJIN_KEYWORD_FEIREN.title"] = "진홍 검";
					dictionary["DANJIN_KEYWORD_FEIREN.description"] = "공격할 때마다 [color=#E84393]진홍 꽃[/color] 1을 얻고, 최대 체력의 [blue]3%[/blue]를 소모합니다(죽지 않음). 체력이 부족할 때도 [color=#E84393]진홍 꽃[/color]을 얻지만, 카드의 수치가 절반이 됩니다.";
					dictionary["DANJIN_KEYWORD_LIAOYU.title"] = "치유";
					dictionary["DANJIN_KEYWORD_LIAOYU.description"] = "[color=#E84393]진홍 꽃[/color]을 소비할 때마다 최대 체력의 [blue]3%[/blue]를 회복합니다. 이번 전투에서 [color=#E84393]진홍 꽃[/color]으로 회복한 체력은 카드로 소모한 체력 총합을 넘을 수 없습니다.";
					dictionary["DANJIN_KEYWORD_LIUZHUAN.title"] = "전환";
					dictionary["DANJIN_KEYWORD_LIUZHUAN.description"] = "현재 [gold]자세[/gold] 수치를 유지한 채, 다른 [gold]자세[/gold]([red]공세[/red] ↔ [blue]수세[/blue])로 전환합니다.";
					dictionary["DANJIN_KEYWORD_CANXIN.title"] = "잔심";
					dictionary["DANJIN_KEYWORD_CANXIN.description"] = "현재 [gold]자세[/gold]를 유지하고, 수치를 [blue]1[/blue] 증가시킵니다.";
					dictionary["DANJIN_KEYWORD_LIANDUAN.title"] = "콤보";
					dictionary["DANJIN_KEYWORD_LIANDUAN.description"] = "[gold]휘발성[/gold], [gold]소멸[/gold]. 다른 카드를 사용한 후, 이 카드는 [gold]소멸[/gold]됩니다.";
					dictionary["DANJIN_KEYWORD_KUJIE.title"] = "고갈";
					dictionary["DANJIN_KEYWORD_KUJIE.description"] = "이번 전투에서 [gold]진홍의 각인[/gold]이 더 이상 발동하지 않으며, [gold]진홍의 각인[/gold]을 부여할 수 없습니다.";
					break;
				case "jpn":
					dictionary["DANJIN_KEYWORD_FEIREN.title"] = "緋刃";
					dictionary["DANJIN_KEYWORD_FEIREN.description"] = "アタックするたびに[color=#E84393]朱華[/color]1を得て、最大HPの[blue]3%[/blue]を消費する(致死しない)。HPが不足する時も[color=#E84393]朱華[/color]を得るが、カードの数値が半分になる。";
					dictionary["DANJIN_KEYWORD_LIAOYU.title"] = "治癒";
					dictionary["DANJIN_KEYWORD_LIAOYU.description"] = "[color=#E84393]朱華[/color]を消費するたびに最大HPの[blue]3%[/blue]を回復する。この戦闘で[color=#E84393]朱華[/color]によって回復したHPは、カードで消費したHPの総量を超えない。";
					dictionary["DANJIN_KEYWORD_LIUZHUAN.title"] = "流転";
					dictionary["DANJIN_KEYWORD_LIUZHUAN.description"] = "現在の[gold]構え[/gold]の数値を維持したまま、別の[gold]構え[/gold]([red]攻勢[/red] ↔ [blue]守勢[/blue])に切り替える。";
					dictionary["DANJIN_KEYWORD_CANXIN.title"] = "残心";
					dictionary["DANJIN_KEYWORD_CANXIN.description"] = "現在の[gold]構え[/gold]を維持し、数値を[blue]1[/blue]増加させる。";
					dictionary["DANJIN_KEYWORD_LIANDUAN.title"] = "コンボ";
					dictionary["DANJIN_KEYWORD_LIANDUAN.description"] = "[gold]エセリアル[/gold]、[gold]廃棄[/gold]。他のカードをプレイした後、このカードは[gold]廃棄[/gold]される。";
					dictionary["DANJIN_KEYWORD_KUJIE.title"] = "枯渇";
					dictionary["DANJIN_KEYWORD_KUJIE.description"] = "この戦闘中、[gold]紅蝕の刻[/gold]はもはや発動せず、[gold]紅蝕の刻[/gold]を付与することができない。";
					break;
				default:
					dictionary["DANJIN_KEYWORD_FEIREN.title"] = "绯刃";
					dictionary["DANJIN_KEYWORD_FEIREN.description"] = "每次攻击获得一点[color=#E84393]彤华[/color]，消耗[blue]3%[/blue]最大生命值(不致死)。生命值不足时仍可获得[color=#E84393]彤华[/color]，但卡牌的数值减半。";
					dictionary["DANJIN_KEYWORD_LIAOYU.title"] = "疗愈";
					dictionary["DANJIN_KEYWORD_LIAOYU.description"] = "每消耗[color=#E84393]彤华[/color]，回复最大生命值的[blue]3%[/blue]。本场战斗中通过[color=#E84393]彤华[/color]回复的生命值不超过卡牌已消耗的总血量。";
					dictionary["DANJIN_KEYWORD_LIUZHUAN.title"] = "流转";
					dictionary["DANJIN_KEYWORD_LIUZHUAN.description"] = "保持当前[gold]架势[/gold]层数，切换为另一种[gold]架势[/gold]([red]攻势[/red]↔[blue]守势[/blue])。";
					dictionary["DANJIN_KEYWORD_CANXIN.title"] = "残心";
					dictionary["DANJIN_KEYWORD_CANXIN.description"] = "保持当前[gold]架势[/gold]，层数[blue]+1[/blue]。";
					dictionary["DANJIN_KEYWORD_LIANDUAN.title"] = "连段";
					dictionary["DANJIN_KEYWORD_LIANDUAN.description"] = "[gold]虚无[/gold]，[gold]消耗[/gold]。打出其他卡牌后，这张牌将被[gold]消耗[/gold]。";
					dictionary["DANJIN_KEYWORD_KUJIE.title"] = "枯竭";
					dictionary["DANJIN_KEYWORD_KUJIE.description"] = "本场战斗[gold]朱蚀之刻[/gold]不再生效，你不再能给予[gold]朱蚀之刻[/gold]。";
					break;
				}
				ClearHoverTipFactoryCaches();
			}
		}

		private static void ClearHoverTipFactoryCaches()
		{
			try
			{
				Type typeFromHandle = typeof(HoverTipFactory);
				string[] array = new string[2] { "_keywordHoverTips", "_potionHoverTips" };
				foreach (string text in array)
				{
					if (AccessTools.Field(typeFromHandle, text)?.GetValue(null) is IDictionary dictionary)
					{
						dictionary.Clear();
					}
				}
			}
			catch (Exception ex)
			{
				Log.Warn("[Danjin] ClearHoverTipFactoryCaches (vanilla) failed: " + ex.Message, 2);
			}
			try
			{
				Type type = AccessTools.TypeByName("STS2RitsuLib.Keywords.Patches.HoverTipFactoryFromKeywordPatch");
				if (type != null && AccessTools.Field(type, "ModKeywordTipCache")?.GetValue(null) is IDictionary dictionary2)
				{
					dictionary2.Clear();
				}
			}
			catch (Exception ex2)
			{
				Log.Warn("[Danjin] ClearHoverTipFactoryCaches (RitsuLib) failed: " + ex2.Message, 2);
			}
		}
	}

	public static readonly CardKeyword FeiRen = ModKeywordRegistry.GetCardKeyword("DANJIN_KEYWORD_FEIREN");

	public static readonly CardKeyword LiaoYu = ModKeywordRegistry.GetCardKeyword("DANJIN_KEYWORD_LIAOYU");

	public static readonly CardKeyword LiuZhuan = ModKeywordRegistry.GetCardKeyword("DANJIN_KEYWORD_LIUZHUAN");

	public static readonly CardKeyword CanXin = ModKeywordRegistry.GetCardKeyword("DANJIN_KEYWORD_CANXIN");

	public static readonly CardKeyword KuJie = ModKeywordRegistry.GetCardKeyword("DANJIN_KEYWORD_KUJIE");

	public static readonly CardKeyword LianDuan = ModKeywordRegistry.GetCardKeyword("DANJIN_KEYWORD_LIANDUAN");

	private DanjinCardKeywords()
	{
	}

	public static void RegisterAll()
	{
		ModKeywordRegistry keywordRegistry = RitsuLibFramework.GetKeywordRegistry("Danjin");
		keywordRegistry.RegisterCardKeywordOwnedByLocNamespace("feiren", (string)null, (ModKeywordCardDescriptionPlacement)1, true);
		keywordRegistry.RegisterCardKeywordOwnedByLocNamespace("liaoyu", (string)null, (ModKeywordCardDescriptionPlacement)1, true);
		keywordRegistry.RegisterCardKeywordOwnedByLocNamespace("liuzhuan", (string)null, (ModKeywordCardDescriptionPlacement)1, true);
		keywordRegistry.RegisterCardKeywordOwnedByLocNamespace("canxin", (string)null, (ModKeywordCardDescriptionPlacement)1, true);
		keywordRegistry.RegisterCardKeywordOwnedByLocNamespace("kujie", (string)null, (ModKeywordCardDescriptionPlacement)2, true);
		keywordRegistry.RegisterCardKeywordOwnedByLocNamespace("lianduan", (string)null, (ModKeywordCardDescriptionPlacement)1, true);
	}
}
