using System.Collections.Generic;
using System.Threading.Tasks;
using Danjin.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Danjin.Cards;

public class AnShuBingFeng : DanjinCard
{
	protected override HashSet<CardTag> CanonicalTags => new HashSet<CardTag> { (CardTag)0 };

	protected override IEnumerable<IHoverTip> AdditionalHoverTips => new _003C_003Ez__ReadOnlyArray<IHoverTip>((IHoverTip[])(object)new IHoverTip[4]
	{
		HoverTipFactory.FromPower<GongShiPower>((int?)null),
		HoverTipFactory.FromPower<ShouShiPower>((int?)null),
		HoverTipFactory.FromPower<StrengthPower>((int?)null),
		HoverTipFactory.FromPower<DexterityPower>((int?)null)
	});

	protected override IEnumerable<DynamicVar> CanonicalVars => new _003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new DynamicVar("CombatExp", 2m));

	public AnShuBingFeng()
		: base(1, (CardType)3, (CardRarity)3, (TargetType)1)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		int num = (int)((CardModel)this).DynamicVars["CombatExp"].BaseValue;
		await PowerCmd.Apply<AnShuBingFengPower>(choiceContext, ((CardModel)this).Owner.Creature, (decimal)num, ((CardModel)this).Owner.Creature, (CardModel)(object)this, false);
		AnShuBingFengPower power = ((CardModel)this).Owner.Creature.GetPower<AnShuBingFengPower>();
		if (power != null)
		{
			await power.Resync(choiceContext);
		}
	}

	protected override void OnUpgrade()
	{
		((CardModel)this).DynamicVars["CombatExp"].UpgradeValueBy(1m);
	}
}
