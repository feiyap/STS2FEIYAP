using Danjin.Extensions;
using Godot;
using STS2RitsuLib.Scaffolding.Content;

namespace Danjin.Character;

public class DanjinCardPool : TypeListCardPoolModel
{
	public override string Title => "Danjin";

	public override string EnergyColorName => "Danjin";

	public override string? BigEnergyIconPath => "Charui/big_energy.png".ImagePath();

	public override string? TextEnergyIconPath => "Charui/text_energy.png".ImagePath();

	public override Material? PoolFrameMaterial => (Material?)(object)DanjinFrameMaterial.DanjinTint();

	public override Color DeckEntryCardColor => new Color("840240");

	public override Color EnergyOutlineColor => new Color("651565");

	public override bool IsColorless => false;
}
