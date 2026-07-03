using MegaCrit.Sts2.Core.Entities.Creatures;

namespace Danjin.Variables;

public interface IFeirenHitCountProvider
{
	int FeirenHitCountForPreview { get; }

	int GetFeirenHitCountForPreview(Creature? target)
	{
		return FeirenHitCountForPreview;
	}
}
