using MegaCrit.Sts2.Core.Entities.Cards;

namespace Danjin.Cards;

public abstract class DanjinTokenCard : DanjinCard
{
	public override bool CanBeGeneratedInCombat => false;

	public override bool CanBeGeneratedByModifiers => false;

	protected DanjinTokenCard(int cost, CardType type, TargetType target)
		: base(cost, type, (CardRarity)7, target, showInCardLibrary: false)
	{
	}//IL_0002: Unknown result type (might be due to invalid IL or missing references)
	//IL_0004: Unknown result type (might be due to invalid IL or missing references)

}
