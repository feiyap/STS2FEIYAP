using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Danjin.Variables;
using Danjin.Vfx;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace Danjin.Cards;

public class DuanHun : DanjinCard
{
	public override IEnumerable<CardKeyword> CanonicalKeywords => new _003C_003Ez__ReadOnlySingleElementList<CardKeyword>(DanjinCardKeywords.FeiRen);

	protected override HashSet<CardTag> CanonicalTags => new HashSet<CardTag> { (CardTag)0 };

	protected override IEnumerable<IHoverTip> AdditionalHoverTips => Array.Empty<IHoverTip>();

	protected override IEnumerable<DynamicVar> CanonicalVars => new _003C_003Ez__ReadOnlyArray<DynamicVar>((DynamicVar[])(object)new DynamicVar[3]
	{
		(DynamicVar)new DuanHunDamageVar(4m),
		new FeiRenVar(),
		new DynamicVar("BonusDmgPerFeiRen", 4m)
	});

	protected override int FeirenHitCount => 1;

	protected override bool ShouldGlowGoldInternal
	{
		get
		{
			if (((CardModel)this).CombatState == null)
			{
				return false;
			}
			return CombatManager.Instance.History.CardPlaysFinished.Any((CardPlayFinishedEntry e) => e.CardPlay.Card.Owner == ((CardModel)this).Owner && e.CardPlay.Card.Keywords.Contains(DanjinCardKeywords.FeiRen));
		}
	}

	public DuanHun()
		: base(1, (CardType)1, (CardRarity)4, (TargetType)2)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, "Target");
		await BeginFeirenAttack(choiceContext);
		int num = CombatManager.Instance.History.CardPlaysFinished.Count((CardPlayFinishedEntry e) => e.CardPlay.Card.Owner == ((CardModel)this).Owner && e.CardPlay.Card.Keywords.Contains(DanjinCardKeywords.FeiRen));
		decimal value = ((DynamicVar)((CardModel)this).DynamicVars.Damage).BaseValue + (decimal)num * ((CardModel)this).DynamicVars["BonusDmgPerFeiRen"].BaseValue;
		value = ScaleByFeiren(value);
		NDimensionSlashVfx slashVfx = PrepareJianQiSlash(cardPlay.Target);
		await DamageCmd.Attack(value).FromCard((CardModel)(object)this).Targeting(cardPlay.Target)
			.BeforeDamage(DanjinCard.DoSlashHook(slashVfx))
			.Execute(choiceContext);
	}

	protected override void OnUpgrade()
	{
		((DynamicVar)((CardModel)this).DynamicVars.Damage).UpgradeValueBy(1m);
		((CardModel)this).DynamicVars["BonusDmgPerFeiRen"].UpgradeValueBy(1m);
	}
}
