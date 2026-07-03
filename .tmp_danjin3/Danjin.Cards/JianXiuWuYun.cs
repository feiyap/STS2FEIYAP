using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Danjin.Extensions;
using Danjin.Vfx;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Danjin.Cards;

public class JianXiuWuYun : DanjinCard
{
	private enum HitCountKind
	{
		KnownMultiple,
		KnownSingle,
		XCost,
		UnknownNoKey
	}

	private const int BaseHitCount = 1;

	private static readonly string[] HitCountKeys = new string[5] { "HitCount", "Repeat", "CalculatedHits", "Hits", "Times" };

	private static readonly string[] DamageKeys = new string[2] { "Damage", "OstyDamage" };

	private static readonly Dictionary<string, int> HardcodedHitCountOverride = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
	{
		["THRASH"] = 2,
		["TWIN_STRIKE"] = 2,
		["DAGGER_SPRAY"] = 2,
		["RIP_AND_TEAR"] = 2,
		["MAUL"] = 2,
		["UPROAR"] = 2
	};

	public override IEnumerable<CardKeyword> CanonicalKeywords => Array.Empty<CardKeyword>();

	protected override HashSet<CardTag> CanonicalTags => new HashSet<CardTag> { (CardTag)0 };

	protected override IEnumerable<IHoverTip> AdditionalHoverTips
	{
		get
		{
			yield return HoverTipFactory.FromKeyword((CardKeyword)1);
			yield return RawHoverTipBuilder.Build("danjin-jianxiuwuyun-canwu", "参悟", "[gold]消耗[/gold]一张攻击牌，若为多段攻击，将攻击段数添加给这张牌；否则将伤害添加给这张牌。");
		}
	}

	protected override IEnumerable<DynamicVar> CanonicalVars => new _003C_003Ez__ReadOnlyArray<DynamicVar>((DynamicVar[])(object)new DynamicVar[2]
	{
		(DynamicVar)new DamageVar(5m, (ValueProp)8),
		new DynamicVar("HitCount", 1m)
	});

	public JianXiuWuYun()
		: base(1, (CardType)1, (CardRarity)4, (TargetType)2)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, "Target");
		if (!cardPlay.Target.IsAlive)
		{
			return;
		}
		decimal baseValue = ((DynamicVar)((CardModel)this).DynamicVars.Damage).BaseValue;
		int num = (int)((CardModel)this).DynamicVars["HitCount"].BaseValue;
		if (num < 1)
		{
			num = 1;
		}
		NDimensionSlashVfx slashVfx = PrepareJianQiSlash(cardPlay.Target, Math.Max(num, 1));
		AttackCommand val = DamageCmd.Attack(baseValue).FromCard((CardModel)(object)this).Targeting(cardPlay.Target)
			.BeforeDamage(DanjinCard.DoSlashHook(slashVfx));
		if (num > 1)
		{
			val = val.WithHitCount(num);
		}
		await val.Execute(choiceContext);
		CardPile pile = PileTypeExtensions.GetPile((PileType)2, ((CardModel)this).Owner);
		List<CardModel> list = ((pile != null) ? pile.Cards.Where((CardModel c) => (object)c != this && (int)c.Type == 1).ToList() : null);
		Player owner = ((CardModel)this).Owner;
		if (list == null || list.Count == 0 || owner == null)
		{
			Log.Info(">>>[DanjinMod] 剑修五蕴：手牌中无其他攻击牌，跳过参悟", 2);
			return;
		}
		CardModel val2 = owner.RunState.Rng.CombatCardSelection.NextItem<CardModel>((IEnumerable<CardModel>)list);
		if (val2 != null)
		{
			var (num2, hitCountKind) = ResolveHitCount(val2);
			if (hitCountKind == HitCountKind.KnownMultiple && num2 > 1)
			{
				DynamicVar obj = ((CardModel)this).DynamicVars["HitCount"];
				obj.BaseValue += (decimal)num2;
				Log.Info($">>>[DanjinMod] 剑修五蕴 / 参悟：[{val2.Title}](多段 {num2})→ 本战斗内 +{num2} 攻击次数，当前 {((CardModel)this).DynamicVars["HitCount"].BaseValue}", 2);
			}
			else
			{
				decimal num3 = ResolveDamage(val2);
				DamageVar damage = ((CardModel)this).DynamicVars.Damage;
				((DynamicVar)damage).BaseValue = ((DynamicVar)damage).BaseValue + num3;
				string value = hitCountKind switch
				{
					HitCountKind.KnownSingle => "单段", 
					HitCountKind.XCost => "X 费/彤华-X 卡，按要求只吸收伤害", 
					_ => "无次数键，按单段处理(可能为硬编码多段，已知限制)", 
				};
				Log.Info($">>>[DanjinMod] 剑修五蕴 / 参悟：[{val2.Title}]({value})→ 本战斗内 +{num3} 伤害，当前 {((DynamicVar)((CardModel)this).DynamicVars.Damage).BaseValue}", 2);
			}
			await CardPileCmd.Add(val2, (PileType)4, (CardPilePosition)1, (AbstractModel)null, false);
		}
	}

	private static (int hitCount, HitCountKind kind) ResolveHitCount(CardModel card)
	{
		if (card == null)
		{
			return (hitCount: 1, kind: HitCountKind.UnknownNoKey);
		}
		CardEnergyCost energyCost = card.EnergyCost;
		if ((energyCost != null && energyCost.CostsX) || card.HasStarCostX)
		{
			return (hitCount: 1, kind: HitCountKind.XCost);
		}
		string entry = ((AbstractModel)card).Id.Entry;
		if (!string.IsNullOrEmpty(entry) && HardcodedHitCountOverride.TryGetValue(entry, out var value))
		{
			if (value < 1)
			{
				value = 1;
			}
			if (value <= 1)
			{
				return (hitCount: 1, kind: HitCountKind.KnownSingle);
			}
			return (hitCount: value, kind: HitCountKind.KnownMultiple);
		}
		if (card.DynamicVars == null)
		{
			return (hitCount: 1, kind: HitCountKind.UnknownNoKey);
		}
		int num = 0;
		bool flag = false;
		string[] hitCountKeys = HitCountKeys;
		DynamicVar val = default(DynamicVar);
		foreach (string text in hitCountKeys)
		{
			if (!card.DynamicVars.TryGetValue(text, ref val) || val == null)
			{
				continue;
			}
			int num2 = (int)val.BaseValue;
			if (num2 >= 1)
			{
				flag = true;
				if (num2 > num)
				{
					num = num2;
				}
			}
		}
		if (!flag)
		{
			return (hitCount: 1, kind: HitCountKind.UnknownNoKey);
		}
		if (num < 1)
		{
			num = 1;
		}
		if (num <= 1)
		{
			return (hitCount: 1, kind: HitCountKind.KnownSingle);
		}
		return (hitCount: num, kind: HitCountKind.KnownMultiple);
	}

	private static decimal ResolveDamage(CardModel card)
	{
		if (((card != null) ? card.DynamicVars : null) == null)
		{
			return 0m;
		}
		string[] damageKeys = DamageKeys;
		DynamicVar val = default(DynamicVar);
		foreach (string text in damageKeys)
		{
			if (card.DynamicVars.TryGetValue(text, ref val) && val != null)
			{
				decimal num = Math.Max(val.BaseValue, 0m);
				if (num > 0m)
				{
					return num;
				}
			}
		}
		return 0m;
	}

	protected override void OnUpgrade()
	{
		((DynamicVar)((CardModel)this).DynamicVars.Damage).UpgradeValueBy(3m);
	}
}
