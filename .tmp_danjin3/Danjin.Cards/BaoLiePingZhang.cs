using System.Collections.Generic;
using System.Threading.Tasks;
using Danjin.Resources;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Danjin.Cards;

public class BaoLiePingZhang : DanjinCard
{
	public override IEnumerable<CardKeyword> CanonicalKeywords => new _003C_003Ez__ReadOnlySingleElementList<CardKeyword>(DanjinCardKeywords.CanXin);

	protected override HashSet<CardTag> CanonicalTags => new HashSet<CardTag> { (CardTag)0 };

	protected override IEnumerable<IHoverTip> AdditionalHoverTips => new _003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.FromPower<FrailPower>((int?)null));

	protected override IEnumerable<DynamicVar> CanonicalVars => new _003C_003Ez__ReadOnlySingleElementList<DynamicVar>((DynamicVar)(object)new PowerVar<FrailPower>(2m));

	public BaoLiePingZhang()
		: base(1, (CardType)1, (CardRarity)2, (TargetType)3)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		Creature creature = ((CardModel)this).Owner.Creature;
		if (creature != null)
		{
			await StanceCmd.ReinforceStance(choiceContext, ((CardModel)this).Owner);
			decimal num = creature.Block;
			if (num > 0m && ((CardModel)this).CombatState != null)
			{
				AoeJianQiSlashGroup slashVfx = PrepareAoeJianQiSlashRandom(1);
				await DamageCmd.Attack(num).FromCard((CardModel)(object)this).TargetingAllOpponents(((CardModel)this).CombatState)
					.BeforeDamage(DanjinCard.DoSlashHook(slashVfx))
					.Execute(choiceContext);
				slashVfx?.ForceComplete();
			}
			await PowerCmd.Apply<FrailPower>(choiceContext, creature, ((CardModel)this).DynamicVars["FrailPower"].BaseValue, creature, (CardModel)(object)this, false);
		}
	}

	protected override void OnUpgrade()
	{
		((CardModel)this).EnergyCost.UpgradeBy(-1);
	}
}
