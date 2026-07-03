using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Danjin.Powers;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Danjin.Cards;

public class TianTong : DanjinCard
{
	public override IEnumerable<CardKeyword> CanonicalKeywords
	{
		get
		{
			if (!((CardModel)this).IsUpgraded)
			{
				return Array.Empty<CardKeyword>();
			}
			return (IEnumerable<CardKeyword>)(object)new CardKeyword[1] { (CardKeyword)5 };
		}
	}

	protected override HashSet<CardTag> CanonicalTags => new HashSet<CardTag> { (CardTag)0 };

	protected override IEnumerable<IHoverTip> AdditionalHoverTips => new _003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.FromPower<StrengthPower>((int?)null));

	protected override IEnumerable<DynamicVar> CanonicalVars => new _003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new DynamicVar("StrengthSteal", 1m));

	protected override bool ShouldGlowGoldInternal
	{
		get
		{
			ICombatState combatState = ((CardModel)this).CombatState;
			if (combatState == null)
			{
				return false;
			}
			return combatState.HittableEnemies.Any(delegate(Creature e)
			{
				if (e.IsAlive)
				{
					MonsterModel monster = e.Monster;
					if (monster == null)
					{
						return false;
					}
					return monster.IntendsToAttack;
				}
				return false;
			});
		}
	}

	public TianTong()
		: base(3, (CardType)3, (CardRarity)4, (TargetType)1)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ICombatState combatState = ((CardModel)this).CombatState;
		IReadOnlyList<Creature> readOnlyList = ((combatState != null) ? combatState.HittableEnemies : null);
		if (readOnlyList != null)
		{
			foreach (Creature item in readOnlyList.ToList())
			{
				if (item.IsAlive && !item.IsStunned)
				{
					MonsterModel monster = item.Monster;
					if (monster != null && monster.IntendsToAttack)
					{
						await CreatureCmd.Stun(item, (string)null);
					}
				}
			}
		}
		int num = (int)((CardModel)this).DynamicVars["StrengthSteal"].BaseValue;
		await PowerCmd.Apply<TianTongPower>(choiceContext, ((CardModel)this).Owner.Creature, (decimal)num, ((CardModel)this).Owner.Creature, (CardModel)(object)this, false);
	}

	protected override void OnUpgrade()
	{
		((CardModel)this).AddKeyword((CardKeyword)5);
	}
}
