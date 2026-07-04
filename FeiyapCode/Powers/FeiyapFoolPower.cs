using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Feiyap.Powers;

/// <summary>
/// 0-愚者：回合开始时根据正逆位触发不同效果。
/// </summary>
[RegisterPower]
public sealed class FeiyapFoolPower : ModPowerTemplate
{
    private bool _isReversed;
    private bool _dualEffect;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    public void SetReversed(bool reversed)
    {
        AssertMutable();
        _isReversed = reversed;
    }

    public void SetDualEffect(bool dual)
    {
        AssertMutable();
        _dualEffect = dual;
    }

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player.Creature != Owner)
        {
            return;
        }

        Flash();
        if (!_isReversed || _dualEffect)
        {
            if (Owner.CombatState is not CombatState state)
            {
                return;
            }

            foreach (var enemy in state.HittableEnemies)
            {
                await PowerCmd.Apply<StrengthPower>(
                    choiceContext,
                    enemy,
                    -1m,
                    Owner,
                    null);
            }
        }

        if (!_dualEffect && _isReversed)
        {
            await ApplyReversedTurnStart(choiceContext, player);
        }
        else if (_dualEffect)
        {
            await ApplyReversedTurnStart(choiceContext, player);
        }
    }

    private static async Task ApplyReversedTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
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
