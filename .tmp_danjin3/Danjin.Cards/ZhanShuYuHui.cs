using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Danjin.Extensions;
using Danjin.Powers;
using Danjin.Resources;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Danjin.Cards;

public class ZhanShuYuHui : DanjinCard
{
	public override IEnumerable<CardKeyword> CanonicalKeywords => new _003C_003Ez__ReadOnlySingleElementList<CardKeyword>(DanjinCardKeywords.CanXin);

	protected override HashSet<CardTag> CanonicalTags => new HashSet<CardTag> { (CardTag)0 };

	protected override IEnumerable<IHoverTip> AdditionalHoverTips => new _003C_003Ez__ReadOnlyArray<IHoverTip>((IHoverTip[])(object)new IHoverTip[2]
	{
		RawHoverTipBuilder.Build("danjin-stance-atk", "攻势1", "打出时需要至少[blue]1[/blue]层[red]攻势[/red]，否则该效果不生效。"),
		RawHoverTipBuilder.Build("danjin-stance-def", "守势1", "打出时需要至少[blue]1[/blue]层[blue]守势[/blue]，否则该效果不生效。")
	});

	protected override IEnumerable<DynamicVar> CanonicalVars => new _003C_003Ez__ReadOnlySingleElementList<DynamicVar>((DynamicVar)new BlockVar(5m, (ValueProp)8));

	public ZhanShuYuHui()
		: base(1, (CardType)2, (CardRarity)3, (TargetType)1)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		Creature creature = ((CardModel)this).Owner.Creature;
		int gongShi = ((creature != null) ? creature.GetPowerAmount<GongShiPower>() : 0);
		Creature creature2 = ((CardModel)this).Owner.Creature;
		int shouShi = ((creature2 != null) ? creature2.GetPowerAmount<ShouShiPower>() : 0);
		await StanceCmd.ReinforceStance(choiceContext, ((CardModel)this).Owner);
		await CreatureCmd.GainBlock(((CardModel)this).Owner.Creature, ((CardModel)this).DynamicVars.Block, cardPlay, false);
		if (gongShi >= 1)
		{
			CardPile pile = PileTypeExtensions.GetPile((PileType)3, ((CardModel)this).Owner);
			if (pile != null && pile.Cards.Count > 0)
			{
				CardSelectorPrefs val = default(CardSelectorPrefs);
				((CardSelectorPrefs)(ref val))._002Ector(((CardModel)this).SelectionScreenPrompt, 1);
				CardModel val2 = (await CardSelectCmd.FromSimpleGrid(choiceContext, pile.Cards, ((CardModel)this).Owner, val)).FirstOrDefault();
				if (val2 != null)
				{
					await CardPileCmd.Add(val2, (PileType)2, (CardPilePosition)1, (AbstractModel)null, false);
				}
			}
		}
		else if (shouShi >= 1)
		{
			await PowerCmd.Apply<ZhanShuYuHuiRetainPower>(choiceContext, ((CardModel)this).Owner.Creature, 1m, ((CardModel)this).Owner.Creature, (CardModel)(object)this, false);
		}
	}

	protected override void OnUpgrade()
	{
		((DynamicVar)((CardModel)this).DynamicVars.Block).UpgradeValueBy(3m);
	}
}
