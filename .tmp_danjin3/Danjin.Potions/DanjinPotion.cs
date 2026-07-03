using Danjin.Character;
using Danjin.Extensions;
using Godot;
using MegaCrit.Sts2.Core.Helpers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Danjin.Potions;

[RegisterPotion(typeof(DanjinPotionPool), Inherit = true)]
public abstract class DanjinPotion : ModPotionTemplate
{
	private string ImageFileBaseName => StringHelper.Slugify(((object)this).GetType().Name).ToLowerInvariant();

	public override string? CustomImagePath
	{
		get
		{
			string text = (ImageFileBaseName + ".png").PotionImagePath();
			if (!ResourceLoader.Exists(text, ""))
			{
				return null;
			}
			return text;
		}
	}

	public override string? CustomOutlinePath
	{
		get
		{
			string text = (ImageFileBaseName + "_outline.png").PotionImagePath();
			if (!ResourceLoader.Exists(text, ""))
			{
				return null;
			}
			return text;
		}
	}
}
