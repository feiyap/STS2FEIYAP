using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Feiyap.Powers;

/// <summary>
/// 0-愚者（逆位）：回合开始时抽技能牌并赋予虚无与减费。
/// </summary>
[RegisterPower]
public sealed class FeiyapFoolReversedPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override PowerAssetProfile AssetProfile =>
        FeiyapPowerAssets.ForSharedIcon(nameof(FeiyapFoolReversedPower), "FeiyapFoolPower");

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player.Creature != Owner)
        {
            return;
        }

        Flash();
        var drawPile = PileType.Draw.GetPile(player);
        var skill = player.RunState.Rng.CombatCardSelection.NextItem(
            drawPile.Cards.Where(c => c.Type == CardType.Skill));

        if (skill == null)
        {
            return;
        }

        await CardPileCmd.Add(skill, PileType.Hand);
        CardCmd.ApplyKeyword(skill, CardKeyword.Ethereal);
        skill.EnergyCost.AddThisTurn(-1, reduceOnly: true);
    }
}
