using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Danjin.Audio;
using Danjin.Powers;
using Danjin.Utils;
using Danjin.Variables;
using Danjin.Vfx;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Danjin.Cards;

public class MangChong : DanjinCard
{
	private static ModSound? _voice;

	public override IEnumerable<CardKeyword> CanonicalKeywords => new _003C_003Ez__ReadOnlySingleElementList<CardKeyword>(DanjinCardKeywords.FeiRen);

	protected override IEnumerable<IHoverTip> AdditionalHoverTips => new _003C_003Ez__ReadOnlyArray<IHoverTip>((IHoverTip[])(object)new IHoverTip[2]
	{
		HoverTipFactory.FromPower<ChuXuePower>((int?)null),
		HoverTipFactory.FromPower<WoundPower>((int?)null)
	});

	protected override HashSet<CardTag> CanonicalTags => new HashSet<CardTag> { (CardTag)0 };

	protected override IEnumerable<DynamicVar> CanonicalVars => new _003C_003Ez__ReadOnlyArray<DynamicVar>((DynamicVar[])(object)new DynamicVar[3]
	{
		(DynamicVar)new DamageVar(6m, (ValueProp)8),
		new FeiRenVar(),
		(DynamicVar)new PowerVar<ChuXuePower>(3m)
	});

	protected override bool ShouldGlowGoldInternal
	{
		get
		{
			ICombatState combatState = ((CardModel)this).CombatState;
			if (combatState == null)
			{
				return false;
			}
			return combatState.HittableEnemies.Any((Creature e) => e.HasPower<WoundPower>());
		}
	}

	public MangChong()
		: base(1, (CardType)1, (CardRarity)3, (TargetType)2)
	{
	}

	protected override int GetFeirenHitCount(Creature? target)
	{
		if (target == null || !target.HasPower<WoundPower>())
		{
			return 1;
		}
		return 2;
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		DanjinVoice.Play(_voice ?? (_voice = new ModSound("res://Danjin/Sounds/Cards/mang_chong.ogg")));
		ArgumentNullException.ThrowIfNull(cardPlay.Target, "Target");
		int hits = GetFeirenHitCount(cardPlay.Target);
		for (int i = 0; i < hits; i++)
		{
			if (!cardPlay.Target.IsAlive)
			{
				break;
			}
			await BeginFeirenAttack(choiceContext, cardPlay.Target);
			decimal num = ScaleByFeiren(((DynamicVar)((CardModel)this).DynamicVars.Damage).BaseValue);
			NDimensionSlashVfx slashVfx = PrepareJianQiSlash(cardPlay.Target);
			slashVfx?.DoSlash();
			await DamageCmd.Attack(num).FromCard((CardModel)(object)this).Targeting(cardPlay.Target)
				.Execute(choiceContext);
			slashVfx?.ForceComplete();
			if (cardPlay.Target.IsAlive)
			{
				decimal num2 = ScaleByFeiren(((CardModel)this).DynamicVars["ChuXuePower"].BaseValue);
				if (num2 > 0m)
				{
					await PowerCmd.Apply<ChuXuePower>(choiceContext, cardPlay.Target, num2, ((CardModel)this).Owner.Creature, (CardModel)(object)this, false);
				}
			}
		}
	}

	protected override void OnUpgrade()
	{
		((DynamicVar)((CardModel)this).DynamicVars.Damage).UpgradeValueBy(1m);
		((CardModel)this).DynamicVars["ChuXuePower"].UpgradeValueBy(1m);
	}
}
