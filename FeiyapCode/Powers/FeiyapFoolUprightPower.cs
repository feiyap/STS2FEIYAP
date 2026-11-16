using System.Linq;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Feiyap.Powers;

/// <summary>
/// 0-愚者（正位）：回合开始时抽 1 张牌，使其本回合耗能降低 1 并附带虚无。
/// 本次赋予的虚无在该牌打出，或因虚无被消耗后移除，不保留到后续抽牌。
/// </summary>
[RegisterPower]
public sealed class FeiyapFoolUprightPower : ModPowerTemplate
{
    private readonly HashSet<CardModel> _grantedEthereal = [];

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
        GrantEtherealUntilPlayed(card);
        card.EnergyCost.AddThisTurn(-1, reduceOnly: true);
    }

    public override Task AfterCardPlayedLate(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        RemoveGrantedEthereal(cardPlay.Card);
        return Task.CompletedTask;
    }

    public override Task AfterCardExhausted(PlayerChoiceContext choiceContext, CardModel card, bool causedByEthereal)
    {
        if (causedByEthereal)
        {
            RemoveGrantedEthereal(card);
        }

        return Task.CompletedTask;
    }

    /// <summary>只记录本次临时加上的虚无，牌本身自带的虚无不移除。</summary>
    private void GrantEtherealUntilPlayed(CardModel card)
    {
        if (card.Keywords.Contains(CardKeyword.Ethereal))
        {
            return;
        }

        CardCmd.ApplyKeyword(card, CardKeyword.Ethereal);
        _grantedEthereal.Add(card);
    }

    private void RemoveGrantedEthereal(CardModel card)
    {
        if (!_grantedEthereal.Remove(card))
        {
            return;
        }

        CardCmd.RemoveKeyword(card, CardKeyword.Ethereal);
    }
}
