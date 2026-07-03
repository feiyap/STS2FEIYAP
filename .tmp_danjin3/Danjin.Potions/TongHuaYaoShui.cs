using System.Collections.Generic;
using System.Threading.Tasks;
using Danjin.Resources;
using Godot;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace Danjin.Potions;

public sealed class TongHuaYaoShui : DanjinPotion
{
	private static readonly Color SplashColor = new Color("e84a8a");

	public override PotionRarity Rarity => (PotionRarity)1;

	public override PotionUsage Usage => (PotionUsage)1;

	public override TargetType TargetType => (TargetType)1;

	protected override IEnumerable<DynamicVar> CanonicalVars => new _003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new DynamicVar("StarAmount", 3m));

	protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
	{
		NCombatRoom instance = NCombatRoom.Instance;
		if (instance != null)
		{
			instance.PlaySplashVfx(((PotionModel)this).Owner.Creature, SplashColor);
		}
		await TonghuaCmd.GainTonghua((int)((PotionModel)this).DynamicVars["StarAmount"].BaseValue, ((PotionModel)this).Owner);
	}
}
