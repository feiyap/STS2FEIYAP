using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Danjin.Audio;
using Danjin.Powers;
using Danjin.Utils;
using Danjin.Variables;
using Danjin.Vfx;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace Danjin.Cards;

public class ZhanJue : DanjinCard
{
	private static ModSound? _voice;

	public override IEnumerable<CardKeyword> CanonicalKeywords => new _003C_003Ez__ReadOnlySingleElementList<CardKeyword>(DanjinCardKeywords.FeiRen);

	protected override HashSet<CardTag> CanonicalTags => new HashSet<CardTag> { (CardTag)0 };

	protected override IEnumerable<IHoverTip> AdditionalHoverTips => Array.Empty<IHoverTip>();

	protected override IEnumerable<DynamicVar> CanonicalVars => new _003C_003Ez__ReadOnlyArray<DynamicVar>((DynamicVar[])(object)new DynamicVar[3]
	{
		(DynamicVar)new ZhanJueDamageVar(7m),
		new FeiRenVar(),
		new DynamicVar("BonusPerSwitch", 3m)
	});

	protected override int FeirenHitCount => 1;

	protected override bool ShouldGlowGoldInternal
	{
		get
		{
			Creature creature = ((CardModel)this).Owner.Creature;
			return ((creature != null && creature.GetPowerAmount<StanceSwitchCountPower>() != 0) ? 1 : 0) > (false ? 1 : 0);
		}
	}

	public ZhanJue()
		: base(1, (CardType)1, (CardRarity)3, (TargetType)2)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		DanjinVoice.Play(_voice ?? (_voice = new ModSound("res://Danjin/Sounds/Cards/zhan_jue.ogg")));
		ArgumentNullException.ThrowIfNull(cardPlay.Target, "Target");
		await BeginFeirenAttack(choiceContext);
		Creature creature = ((CardModel)this).Owner.Creature;
		int num = ((creature != null) ? creature.GetPowerAmount<StanceSwitchCountPower>() : 0);
		decimal value = ((DynamicVar)((CardModel)this).DynamicVars.Damage).BaseValue + (decimal)num * ((CardModel)this).DynamicVars["BonusPerSwitch"].BaseValue;
		value = ScaleByFeiren(value);
		NDimensionSlashVfx slashVfx = PrepareJianQiSlash(cardPlay.Target);
		await DamageCmd.Attack(value).FromCard((CardModel)(object)this).Targeting(cardPlay.Target)
			.BeforeDamage(DanjinCard.DoSlashHook(slashVfx))
			.Execute(choiceContext);
	}

	protected override void OnUpgrade()
	{
		((CardModel)this).DynamicVars["BonusPerSwitch"].UpgradeValueBy(1m);
	}
}
