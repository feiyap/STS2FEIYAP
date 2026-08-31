using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Feiyap.Powers;

/// <summary>
/// 无我梦中：对标记目标伤害提升；其他敌人对你的伤害降低。Amount 为百分比。
/// </summary>
[RegisterPower]
public sealed class FeiyapMugaMuchuPower : ModPowerTemplate
{
    private Creature? _markedTarget;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override PowerAssetProfile AssetProfile =>
        FeiyapPowerAssets.ForSharedIcon(nameof(FeiyapMugaMuchuPower), "FeiyapSwordSaintHeartPower");

    public Creature? MarkedTarget
    {
        get => _markedTarget;
        set => _markedTarget = value;
    }

    public override decimal ModifyDamageMultiplicative(
        Creature? target,
        decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource,
        CardPlay? cardPlay)
    {
        if (Amount <= 0m)
        {
            return 1m;
        }

        var ratio = Amount / 100m;

        if (dealer == Owner && target != null && target == _markedTarget)
        {
            return 1m + ratio;
        }

        if (target == Owner
            && dealer != null
            && dealer.IsMonster
            && dealer != _markedTarget)
        {
            return Math.Max(0m, 1m - ratio);
        }

        return 1m;
    }

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player.Creature != Owner)
        {
            return;
        }

        await PowerCmd.Remove(this);
    }
}
