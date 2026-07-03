using System.Collections.Generic;
using System.Threading.Tasks;
using Danjin.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace Danjin.Cards;

public class ShengFang : DanjinCard
{
	protected override HashSet<CardTag> CanonicalTags => new HashSet<CardTag> { (CardTag)0 };

	protected override IEnumerable<IHoverTip> AdditionalHoverTips => new _003C_003Ez__ReadOnlyArray<IHoverTip>((IHoverTip[])(object)new IHoverTip[2]
	{
		HoverTipFactory.FromPower<ZhuShiZhiKePower>((int?)null),
		HoverTipFactory.FromPower<ChuXuePower>((int?)null)
	});

	protected override IEnumerable<DynamicVar> CanonicalVars => new _003C_003Ez__ReadOnlyArray<DynamicVar>((DynamicVar[])(object)new DynamicVar[2]
	{
		new DynamicVar("DrawAmount", 1m),
		new DynamicVar("BleedStacks", 1m)
	});

	public ShengFang()
		: base(1, (CardType)3, (CardRarity)3, (TargetType)1)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		int num = (int)((CardModel)this).DynamicVars["DrawAmount"].BaseValue;
		await PowerCmd.Apply<ShengFangPower>(choiceContext, ((CardModel)this).Owner.Creature, (decimal)num, ((CardModel)this).Owner.Creature, (CardModel)(object)this, false);
	}

	protected override void OnUpgrade()
	{
		((CardModel)this).DynamicVars["DrawAmount"].UpgradeValueBy(1m);
	}
}
