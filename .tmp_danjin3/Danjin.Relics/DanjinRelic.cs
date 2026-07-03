using Danjin.Character;
using Danjin.Extensions;
using Godot;
using MegaCrit.Sts2.Core.Helpers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Danjin.Relics;

[RegisterRelic(typeof(DanjinRelicPool), Inherit = true)]
public abstract class DanjinRelic : ModRelicTemplate
{
	private string IconFileBaseName => StringHelper.Slugify(((object)this).GetType().Name).ToLowerInvariant();

	public override string? CustomIconPath
	{
		get
		{
			string text = (IconFileBaseName + ".png").RelicImagePath();
			if (!ResourceLoader.Exists(text, ""))
			{
				return "relic.png".RelicImagePath();
			}
			return text;
		}
	}

	public override string? CustomIconOutlinePath
	{
		get
		{
			string text = (IconFileBaseName + "_outline.png").RelicImagePath();
			if (!ResourceLoader.Exists(text, ""))
			{
				return "relic_outline.png".RelicImagePath();
			}
			return text;
		}
	}

	public override string? CustomBigIconPath
	{
		get
		{
			string text = (IconFileBaseName + ".png").BigRelicImagePath();
			if (!ResourceLoader.Exists(text, ""))
			{
				return "relic.png".BigRelicImagePath();
			}
			return text;
		}
	}
}
