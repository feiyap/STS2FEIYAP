using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Feiyap.Powers;

/// <summary>
/// 百巧手：打出攻击牌获得临时敏捷，打出技能牌获得临时力量。
/// </summary>
[RegisterPower]
public sealed class FeiyapHyakukyoushiPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => FeiyapPowerAssets.For(nameof(FeiyapHyakukyoushiPower));

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner?.Creature != Owner || Amount <= 0)
        {
            return;
        }

        Flash();
        if (cardPlay.Card.Type == CardType.Attack)
        {
            await PowerCmd.Apply<AnticipatePower>(
                choiceContext,
                Owner,
                Amount,
                Owner,
                cardPlay.Card);
            return;
        }

        if (cardPlay.Card.Type == CardType.Skill)
        {
            await PowerCmd.Apply<SetupStrikePower>(
                choiceContext,
                Owner,
                Amount,
                Owner,
                cardPlay.Card);
        }
    }
}
