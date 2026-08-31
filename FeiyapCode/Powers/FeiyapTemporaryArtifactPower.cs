using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Feiyap.Powers;

/// <summary>
/// 侘寂：自己的手牌/抽牌堆/弃牌堆被加入状态牌时，移除 1 层并消耗该牌。
/// </summary>
[RegisterPower]
public sealed class FeiyapTemporaryArtifactPower : ModPowerTemplate
{
    private bool _resolving;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => FeiyapPowerAssets.For(nameof(FeiyapTemporaryArtifactPower));

    public override async Task AfterCardChangedPiles(CardModel card, PileType oldPileType, AbstractModel? clonedBy)
    {
        if (_resolving
            || Amount <= 0m
            || card.Type != CardType.Status
            || card.Owner?.Creature != Owner)
        {
            return;
        }

        var newPile = card.Pile?.Type;
        if (newPile is not (PileType.Hand or PileType.Draw or PileType.Discard))
        {
            return;
        }

        // 三区间内部流转（抽牌、弃牌等）不视为「加入」。
        if (oldPileType is PileType.Hand or PileType.Draw or PileType.Discard or PileType.Play)
        {
            return;
        }

        _resolving = true;
        try
        {
            Flash();
            await PowerCmd.Decrement(this);
            await CardCmd.Exhaust(new ThrowingPlayerChoiceContext(), card);
        }
        finally
        {
            _resolving = false;
        }
    }
}
