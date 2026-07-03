using Godot;
using STS2RitsuLib.Scaffolding.Content;

namespace Danjin.Character;

public class DanjinPotionPool : TypeListPotionPoolModel
{
	public override string EnergyColorName => "Danjin";

	public override Color LabOutlineColor => Danjin.Color;
}
