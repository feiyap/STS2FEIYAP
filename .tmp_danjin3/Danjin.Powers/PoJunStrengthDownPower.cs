using Danjin.Cards;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Danjin.Powers;

public sealed class PoJunStrengthDownPower : DanjinTempPower<PoJun, StrengthPower>
{
	protected override bool IsPositive => false;
}
