using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Feiyap.Powers;

/// <summary>
/// 连续施法：本回合每次交替打出攻击/技能牌时抽 1 张牌。
/// </summary>
[RegisterPower]
public sealed class FeiyapContinuousCastPower : FeiyapAlternateCastPowerBase
{
    protected override Task OnAlternateCast(PlayerChoiceContext choiceContext, CardPlay cardPlay) =>
        CardPileCmd.Draw(choiceContext, 1, cardPlay.Card.Owner);
}
