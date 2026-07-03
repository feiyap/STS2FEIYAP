using System.Collections.Generic;
using System.Threading.Tasks;
using Danjin.Powers;
using Danjin.Resources;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Danjin.Cards;

public class XinWuPangWu : DanjinCard
{
	public override IEnumerable<CardKeyword> CanonicalKeywords => new _003C_003Ez__ReadOnlySingleElementList<CardKeyword>(DanjinCardKeywords.CanXin);

	public override int CanonicalStarCost => 2;

	protected override HashSet<CardTag> CanonicalTags => new HashSet<CardTag> { (CardTag)0 };

	protected override IEnumerable<DynamicVar> CanonicalVars => new _003C_003Ez__ReadOnlySingleElementList<DynamicVar>((DynamicVar)new BlockVar(7m, (ValueProp)8));

	public XinWuPangWu()
		: base(0, (CardType)2, (CardRarity)2, (TargetType)1)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await StanceCmd.ReinforceStance(choiceContext, ((CardModel)this).Owner);
		await CreatureCmd.GainBlock(((CardModel)this).Owner.Creature, ((CardModel)this).DynamicVars.Block, cardPlay, false);
		Creature creature = ((CardModel)this).Owner.Creature;
		int num;
		if (creature == null || !creature.HasPower<KuangLuanPower>())
		{
			Creature creature2 = ((CardModel)this).Owner.Creature;
			num = ((creature2 != null && creature2.HasPower<QianJunPower>()) ? 1 : 0);
		}
		else
		{
			num = 1;
		}
		if (num == 0)
		{
			await PowerCmd.Apply<StanceLockPower>(choiceContext, ((CardModel)this).Owner.Creature, 1m, ((CardModel)this).Owner.Creature, (CardModel)(object)this, false);
		}
	}

	protected override void OnUpgrade()
	{
		((DynamicVar)((CardModel)this).DynamicVars.Block).UpgradeValueBy(3m);
	}
}
