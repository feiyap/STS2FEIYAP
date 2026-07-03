using System.Collections.Generic;
using System.Threading.Tasks;
using Danjin.Extensions;
using Danjin.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Danjin.Cards;

public class LiBingMoMa : DanjinCard
{
	protected override HashSet<CardTag> CanonicalTags => new HashSet<CardTag> { (CardTag)0 };

	protected override IEnumerable<IHoverTip> AdditionalHoverTips => new _003C_003Ez__ReadOnlyArray<IHoverTip>((IHoverTip[])(object)new IHoverTip[4]
	{
		HoverTipFactory.FromPower<StrengthPower>((int?)null),
		HoverTipFactory.FromPower<DexterityPower>((int?)null),
		RawHoverTipBuilder.Build("danjin-stance-atk", "攻势1", "打出时需要至少[blue]1[/blue]层[red]攻势[/red]，否则该效果不生效。"),
		RawHoverTipBuilder.Build("danjin-stance-def", "守势1", "打出时需要至少[blue]1[/blue]层[blue]守势[/blue]，否则该效果不生效。")
	});

	protected override IEnumerable<DynamicVar> CanonicalVars => new _003C_003Ez__ReadOnlySingleElementList<DynamicVar>((DynamicVar)new CardsVar(3));

	public LiBingMoMa()
		: base(1, (CardType)2, (CardRarity)3, (TargetType)1)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		Creature creature = ((CardModel)this).Owner.Creature;
		int gongShi = ((creature != null) ? creature.GetPowerAmount<GongShiPower>() : 0);
		Creature creature2 = ((CardModel)this).Owner.Creature;
		int shouShi = ((creature2 != null) ? creature2.GetPowerAmount<ShouShiPower>() : 0);
		await CardPileCmd.Draw(choiceContext, ((DynamicVar)((CardModel)this).DynamicVars.Cards).BaseValue, ((CardModel)this).Owner, false);
		if (gongShi >= 1)
		{
			await PowerCmd.Apply<LiBingMoMaStrengthPower>(choiceContext, ((CardModel)this).Owner.Creature, 1m, ((CardModel)this).Owner.Creature, (CardModel)(object)this, false);
		}
		else if (shouShi >= 1)
		{
			await PowerCmd.Apply<LiBingMoMaDexterityPower>(choiceContext, ((CardModel)this).Owner.Creature, 1m, ((CardModel)this).Owner.Creature, (CardModel)(object)this, false);
		}
	}

	protected override void OnUpgrade()
	{
		((DynamicVar)((CardModel)this).DynamicVars.Cards).UpgradeValueBy(1m);
	}
}
