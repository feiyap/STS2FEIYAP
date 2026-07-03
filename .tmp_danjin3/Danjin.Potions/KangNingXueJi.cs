using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Danjin.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace Danjin.Potions;

public sealed class KangNingXueJi : DanjinPotion
{
	public override PotionRarity Rarity => (PotionRarity)2;

	public override PotionUsage Usage => (PotionUsage)1;

	public override TargetType TargetType => (TargetType)2;

	protected override IEnumerable<DynamicVar> CanonicalVars => Array.Empty<DynamicVar>();

	protected override IEnumerable<IHoverTip> AdditionalHoverTips => new _003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.FromPower<KangNingXuePower>((int?)null));

	protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
	{
		PotionModel.AssertValidForTargetedPotion(target);
		await PowerCmd.Apply<KangNingXuePower>(choiceContext, target, 1m, ((PotionModel)this).Owner.Creature, (CardModel)null, false);
	}
}
