using System;
using MegaCrit.Sts2.Core.Models;

namespace Danjin.Patches;

public static class CardContext
{
	[ThreadStatic]
	public static CardModel? CurrentFormattingCard;

	[ThreadStatic]
	public static CardModel? CurrentPlayingCard;
}
