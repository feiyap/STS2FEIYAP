using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Danjin.Audio;
using Danjin.Resources;
using Danjin.Utils;
using Danjin.Variables;
using Danjin.Vfx;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Danjin.Cards;

public class ChiHua : DanjinCard
{
	private static ModSound? _voice;

	public override IEnumerable<CardKeyword> CanonicalKeywords => new _003C_003Ez__ReadOnlyArray<CardKeyword>((CardKeyword[])(object)new CardKeyword[2]
	{
		(CardKeyword)(int)DanjinCardKeywords.FeiRen,
		(CardKeyword)3
	});

	protected override HashSet<CardTag> CanonicalTags => new HashSet<CardTag> { (CardTag)0 };

	protected override IEnumerable<DynamicVar> CanonicalVars => new _003C_003Ez__ReadOnlyArray<DynamicVar>((DynamicVar[])(object)new DynamicVar[4]
	{
		(DynamicVar)new DamageVar(10m, (ValueProp)8),
		new DynamicVar("HitCount", 2m),
		new FeiRenVar(),
		new ZhuShiZhiKeVar(2m)
	});

	protected override int FeirenHitCount => (int)((CardModel)this).DynamicVars["HitCount"].BaseValue;

	protected override IEnumerable<IHoverTip> AdditionalHoverTips
	{
		get
		{
			yield return HoverTipFactory.FromPower<ZhuShiZhiKePower>((int?)null);
		}
	}

	public ChiHua()
		: base(1, (CardType)1, (CardRarity)5, (TargetType)2)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
		DanjinVoice.Play(_voice ?? (_voice = new ModSound("res://Danjin/Sounds/Cards/jian_qi.ogg")));
		await BeginFeirenAttack(choiceContext);
		int num = (int)((CardModel)this).DynamicVars["HitCount"].BaseValue;
		decimal num2 = ScaleByFeiren(((DynamicVar)((CardModel)this).DynamicVars.Damage).BaseValue);
		NDimensionSlashVfx slashVfx = PrepareJianQiSlash(cardPlay.Target, num);
		if (cardPlay.Target.IsAlive)
		{
			await DamageCmd.Attack(num2).WithHitCount(num).FromCard((CardModel)(object)this)
				.Targeting(cardPlay.Target)
				.BeforeDamage(DanjinCard.DoSlashHook(slashVfx))
				.Execute(choiceContext);
		}
		slashVfx?.ForceComplete();
		if (cardPlay.Target.IsAlive)
		{
			decimal num3 = ScaleByFeiren(((CardModel)this).DynamicVars["ZhuShiZhiKeStacks"].BaseValue);
			if (num3 > 0m)
			{
				await PowerCmd.Apply<ZhuShiZhiKePower>(choiceContext, cardPlay.Target, num3, ((CardModel)this).Owner.Creature, (CardModel)(object)this, false);
			}
		}
		await TonghuaCmd.GainTonghua(1m, ((CardModel)this).Owner);
	}

	protected override void OnUpgrade()
	{
		((DynamicVar)((CardModel)this).DynamicVars.Damage).UpgradeValueBy(5m);
		((CardModel)this).DynamicVars["ZhuShiZhiKeStacks"].UpgradeValueBy(2m);
	}
}
