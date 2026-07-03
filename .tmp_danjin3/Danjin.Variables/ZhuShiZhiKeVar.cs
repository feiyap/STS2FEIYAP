using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace Danjin.Variables;

public class ZhuShiZhiKeVar : DynamicVar
{
	public const string DefaultName = "ZhuShiZhiKeStacks";

	public decimal BaseStacks { get; }

	public ZhuShiZhiKeVar(decimal baseStacks = 1m)
		: base("ZhuShiZhiKeStacks", baseStacks)
	{
		BaseStacks = baseStacks;
	}

	public override void UpdateCardPreview(CardModel card, CardPreviewMode previewMode, Creature? target, bool runGlobalHooks)
	{
		((DynamicVar)this).PreviewValue = ((DynamicVar)this).BaseValue;
	}
}
