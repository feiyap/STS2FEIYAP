using MegaCrit.Sts2.Core.Entities.Creatures;

namespace Danjin.Hooks;

public interface IDanjinHealAmountModifier
{
	decimal ModifyHealAdditive(Creature creature, decimal amount)
	{
		return 0m;
	}

	decimal ModifyHealMultiplicative(Creature creature, decimal amount)
	{
		return 1m;
	}
}
