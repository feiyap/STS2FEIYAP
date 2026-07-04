using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Feiyap.Mechanics;

/// <summary>
/// 居合动态变量：预览时应用敏捷、残心等与 <see cref="FeiyapIaidoCmd.Gain"/> 一致的修正。
/// </summary>
public sealed class IaidoVar : DynamicVar
{
    public const string DefaultName = "Iaido";

    public ValueProp Props { get; }

    public IaidoVar(decimal iaido, ValueProp props)
        : base(DefaultName, iaido)
    {
        Props = props;
    }

    public IaidoVar(string name, decimal iaido, ValueProp props)
        : base(name, iaido)
    {
        Props = props;
    }

    public override void UpdateCardPreview(
        CardModel card,
        CardPreviewMode previewMode,
        Creature? target,
        bool runGlobalHooks)
    {
        var amount = BaseValue;

        if (runGlobalHooks && card.Owner?.Creature is { } creature)
        {
            amount = FeiyapIaidoCmd.PreviewGain(creature, BaseValue, Props, card);
        }

        PreviewValue = amount;
    }
}
