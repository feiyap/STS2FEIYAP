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

namespace Danjin.Cards;

public class ShouHuoShiKe : DanjinCard
{
	public override IEnumerable<CardKeyword> CanonicalKeywords => new _003C_003Ez__ReadOnlyArray<CardKeyword>((CardKeyword[])(object)new CardKeyword[2]
	{
		(CardKeyword)1,
		(CardKeyword)(int)DanjinCardKeywords.KuJie
	});

	protected override HashSet<CardTag> CanonicalTags => new HashSet<CardTag> { (CardTag)0 };

	protected override IEnumerable<IHoverTip> AdditionalHoverTips => new _003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.FromPower<ZhuShiZhiKePower>((int?)null));

	protected override IEnumerable<DynamicVar> CanonicalVars => new _003C_003Ez__ReadOnlySingleElementList<DynamicVar>((DynamicVar)new EnergyVar(1));

	protected override bool IsPlayable
	{
		get
		{
			ICombatState combatState = ((CardModel)this).CombatState;
			if (combatState == null)
			{
				return false;
			}
			return combatState.HittableEnemies.Any((Creature e) => e.HasPower<ZhuShiZhiKePower>());
		}
	}

	protected override bool ShouldGlowGoldInternal => ((CardModel)this).IsPlayable;

	public ShouHuoShiKe()
		: base(1, (CardType)2, (CardRarity)4, (TargetType)3)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		int totalMarks = 0;
		ICombatState combatState = ((CardModel)this).CombatState;
		List<Creature> list = ((combatState != null) ? combatState.HittableEnemies.Where((Creature e) => e.IsAlive).ToList() : null);
		if (list != null)
		{
			foreach (Creature item in list)
			{
				ZhuShiZhiKePower power = item.GetPower<ZhuShiZhiKePower>();
				int num = ((power != null) ? ((PowerModel)power).Amount : 0);
				if (num > 0)
				{
					totalMarks += num;
					await PowerCmd.Apply<ZhuShiZhiKePower>(choiceContext, item, -(decimal)num, ((CardModel)this).Owner.Creature, (CardModel)(object)this, false);
				}
			}
		}
		if (totalMarks > 0)
		{
			int capped = ((totalMarks > 10) ? 10 : totalMarks);
			await PlayerCmd.GainEnergy((decimal)capped, ((CardModel)this).Owner);
			await CardPileCmd.Draw(choiceContext, (decimal)capped, ((CardModel)this).Owner, false);
		}
		await PowerCmd.Apply<ZhuShiInvalidatedPower>(choiceContext, ((CardModel)this).Owner.Creature, 1m, ((CardModel)this).Owner.Creature, (CardModel)(object)this, false);
	}

	protected override void OnUpgrade()
	{
		((CardModel)this).EnergyCost.UpgradeBy(-1);
	}
}
