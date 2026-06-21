using System.Collections.Generic;
using System.Threading.Tasks;
using Feiyap.Mechanics;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Scaffolding.Content;

namespace Feiyap.Powers;

/// <summary>
/// 本回合交替打出攻击/技能牌时触发的临时能力基类。
/// </summary>
public abstract class FeiyapAlternateCastPowerBase : ModPowerTemplate
{
    private CardType? _lastPlayedType;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    /// <summary>从战斗追踪器同步上一张打出牌的类型。</summary>
    public void SyncLastPlayedType(Player player)
    {
        _lastPlayedType = FeiyapCombatTracker.Get(player).LastPlayedType;
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner?.Creature != Owner)
        {
            return;
        }

        var type = cardPlay.Card.Type;
        if (type is not (CardType.Attack or CardType.Skill))
        {
            return;
        }

        if (_lastPlayedType is CardType.Attack or CardType.Skill && _lastPlayedType != type)
        {
            await OnAlternateCast(choiceContext, cardPlay);
        }

        _lastPlayedType = type;
    }

    public override async Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (!participants.Contains(Owner))
        {
            return;
        }

        await PowerCmd.Remove(this);
    }

    protected abstract Task OnAlternateCast(PlayerChoiceContext choiceContext, CardPlay cardPlay);
}
