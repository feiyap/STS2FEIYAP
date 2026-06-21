using System.Threading.Tasks;
using Feiyap.Mechanics;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Feiyap.Powers;

/// <summary>
/// 转换施法：本回合每次交替打出攻击/技能牌时获得残心。
/// </summary>
[RegisterPower]
public sealed class FeiyapConversionCastPower : FeiyapAlternateCastPowerBase
{
    protected override Task OnAlternateCast(PlayerChoiceContext choiceContext, CardPlay cardPlay) =>
        FeiyapZanxinCmd.Gain(
            choiceContext,
            cardPlay.Card.Owner.Creature,
            Amount,
            cardPlay.Card);
}
