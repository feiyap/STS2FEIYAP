using System.Collections.Generic;
using System.Threading.Tasks;
using Danjin.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Danjin.Cards;

public class BuDongRuShan : DanjinCard
{
	protected override HashSet<CardTag> CanonicalTags => new HashSet<CardTag> { (CardTag)0 };

	protected override IEnumerable<IHoverTip> AdditionalHoverTips => new _003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.FromPower<ShouShiPower>((int?)null));

	protected override IEnumerable<DynamicVar> CanonicalVars => new _003C_003Ez__ReadOnlyArray<DynamicVar>((DynamicVar[])(object)new DynamicVar[2]
	{
		(DynamicVar)new BlockVar(14m, (ValueProp)8),
		new DynamicVar("BlockPerShouShi", 3m)
	});

	public BuDongRuShan()
		: base(2, (CardType)2, (CardRarity)3, (TargetType)1)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		Creature creature = ((CardModel)this).Owner.Creature;
		int num = ((creature != null) ? creature.GetPowerAmount<ShouShiPower>() : 0);
		decimal num2 = ((DynamicVar)((CardModel)this).DynamicVars.Block).BaseValue + (decimal)num * ((CardModel)this).DynamicVars["BlockPerShouShi"].BaseValue;
		await CreatureCmd.GainBlock(((CardModel)this).Owner.Creature, num2, (ValueProp)8, cardPlay, false);
	}

	protected override void OnUpgrade()
	{
		((DynamicVar)((CardModel)this).DynamicVars.Block).UpgradeValueBy(2m);
		((CardModel)this).DynamicVars["BlockPerShouShi"].UpgradeValueBy(1m);
	}
}
