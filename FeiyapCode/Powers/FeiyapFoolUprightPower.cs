using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Feiyap.Powers;

/// <summary>
/// 0-愚者（正位）：回合开始时抽 1 张牌，使其本回合耗能降低 1 并附带虚无。
/// </summary>
[RegisterPower]
public sealed class FeiyapFoolUprightPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override PowerAssetProfile AssetProfile =>
        FeiyapPowerAssets.ForSharedIcon(nameof(FeiyapFoolUprightPower), "FeiyapFoolPower");

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player.Creature != Owner)
        {
            return;
        }

        Flash();
        var drawPile = PileType.Draw.GetPile(player);
        var card = player.RunState.Rng.CombatCardSelection.NextItem(drawPile.Cards);
        if (card == null)
        {
            return;
        }

        await CardPileCmd.Add(card, PileType.Hand);
        CardCmd.ApplyKeyword(card, CardKeyword.Ethereal);
        card.EnergyCost.AddThisTurn(-1, reduceOnly: true);
    }
}
