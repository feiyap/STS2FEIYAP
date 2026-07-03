using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Danjin.Powers;
using Danjin.Vfx;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Danjin.Cards;

public class JiLiuSongZang : DanjinCard
{
	protected override bool HasEnergyCostX => true;

	public override IEnumerable<CardKeyword> CanonicalKeywords => Array.Empty<CardKeyword>();

	protected override HashSet<CardTag> CanonicalTags => new HashSet<CardTag> { (CardTag)0 };

	protected override IEnumerable<IHoverTip> AdditionalHoverTips => new _003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.FromPower<ChuXuePower>((int?)null));

	protected override IEnumerable<DynamicVar> CanonicalVars => new _003C_003Ez__ReadOnlyArray<DynamicVar>((DynamicVar[])(object)new DynamicVar[2]
	{
		(DynamicVar)new DamageVar(4m, (ValueProp)8),
		(DynamicVar)new PowerVar<ChuXuePower>(3m)
	});

	protected override bool ShouldGlowGoldInternal
	{
		get
		{
			Player owner = ((CardModel)this).Owner;
			int? obj;
			if (owner == null)
			{
				obj = null;
			}
			else
			{
				PlayerCombatState playerCombatState = owner.PlayerCombatState;
				obj = ((playerCombatState != null) ? new int?(playerCombatState.Energy) : ((int?)null));
			}
			int? num = obj;
			return num.GetValueOrDefault() >= 4;
		}
	}

	public JiLiuSongZang()
		: base(0, (CardType)1, (CardRarity)4, (TargetType)2)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		int num = ((CardModel)this).ResolveEnergyXValue();
		if (num > 0)
		{
			int num2 = ((num >= 4) ? (num * 2) : num);
			NDimensionSlashVfx slashVfx = PrepareJianQiSlash(cardPlay.Target, num2);
			await DamageCmd.Attack(((DynamicVar)((CardModel)this).DynamicVars.Damage).BaseValue).WithHitCount(num2).FromCard((CardModel)(object)this)
				.Targeting(cardPlay.Target)
				.BeforeDamage((Func<Task>)async delegate
				{
					slashVfx?.DoSlash();
					await PowerCmd.Apply<ChuXuePower>(choiceContext, cardPlay.Target, ((CardModel)this).DynamicVars["ChuXuePower"].BaseValue, ((CardModel)this).Owner.Creature, (CardModel)(object)this, false);
				})
				.Execute(choiceContext);
			slashVfx?.ForceComplete();
		}
	}

	protected override void OnUpgrade()
	{
		((DynamicVar)((CardModel)this).DynamicVars.Damage).UpgradeValueBy(1m);
		((CardModel)this).DynamicVars["ChuXuePower"].UpgradeValueBy(1m);
	}
}
