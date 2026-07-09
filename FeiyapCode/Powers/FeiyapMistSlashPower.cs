using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Feiyap.Powers;

/// <summary>
/// 迷雾斩：下一次段数大于 1 的攻击牌获得额外攻击段数。
/// </summary>
[RegisterPower]
public sealed class FeiyapMistSlashPower : ModPowerTemplate
{
    private bool _pendingConsume;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => FeiyapPowerAssets.For(nameof(FeiyapMistSlashPower));

    public override int ModifyAttackHitCount(AttackCommand attack, int hitCount)
    {
        if (Amount <= 0 || attack.Attacker != Owner || hitCount <= 1)
        {
            return hitCount;
        }

        if (attack.ModelSource is not CardModel card || card.Type != CardType.Attack)
        {
            return hitCount;
        }

        _pendingConsume = true;
        return hitCount + Amount;
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (!_pendingConsume || cardPlay.Card.Owner?.Creature != Owner)
        {
            return;
        }

        _pendingConsume = false;
        Flash();
        await PowerCmd.Remove(this);
    }
}
