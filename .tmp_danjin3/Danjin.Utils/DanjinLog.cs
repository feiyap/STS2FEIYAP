using MegaCrit.Sts2.Core.Logging;

namespace Danjin.Utils;

public static class DanjinLog
{
	public static bool EnableVerbose;

	public static void Verbose(string msg)
	{
		if (EnableVerbose)
		{
			Log.Info(msg, 2);
		}
	}
}
