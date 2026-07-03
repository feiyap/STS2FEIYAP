using Danjin.Extensions;
using Godot;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Combat.Powers;
using STS2RitsuLib.Interop.AutoRegistration;

namespace Danjin.Powers;

[RegisterPower(Inherit = true)]
public abstract class DanjinTempPower<TOrigin, TPower> : ModTemporaryAppliedPowerTemplate<TOrigin, TPower>, IDanjinTempPower where TOrigin : AbstractModel where TPower : PowerModel
{
	private string KeySuffix
	{
		get
		{
			if (((PowerModel)this).Amount > 0 != ((ModTemporaryPowerTemplate)this).IsPositive)
			{
				return "DOWN";
			}
			return "UP";
		}
	}

	public override LocString Description => new LocString("powers", "DANJIN_POWER_TEMP_POWER." + KeySuffix + ".description");

	protected override string SmartDescriptionLocKey => "DANJIN_POWER_TEMP_POWER." + KeySuffix + ".smartDescription";

	private string IconFileBaseName => StringHelper.Slugify(((object)this).GetType().Name).ToLowerInvariant();

	public override string? CustomIconPath
	{
		get
		{
			string text = (IconFileBaseName + ".png").PowerImagePath();
			if (!ResourceLoader.Exists(text, ""))
			{
				return "power.png".PowerImagePath();
			}
			return text;
		}
	}

	public override string? CustomBigIconPath
	{
		get
		{
			string text = (IconFileBaseName + ".png").BigPowerImagePath();
			if (!ResourceLoader.Exists(text, ""))
			{
				return "power.png".BigPowerImagePath();
			}
			return text;
		}
	}
}
