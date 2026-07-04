using Feiyap.Cards.Tarot;
using Feiyap.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Scaffolding.Characters;

namespace Feiyap.Mechanics;

/// <summary>
/// 塔罗牌共用逻辑：XXI-世界 等效果下的正逆位选择与分支执行。
/// </summary>
public static class FeiyapTarotCmd
{
    public static bool HasFreeChoice(Player? player) =>
        player?.Creature.FindPower<FeiyapTarotWorldFreeChoicePower>() != null;

    public static bool HasDualEffect(Player? player) =>
        player?.Creature.FindPower<FeiyapTarotWorldDualEffectPower>() != null;

    /// <summary>让玩家在正位与逆位效果间选择，返回 true 表示正位。</summary>
    public static async Task<bool> ChooseUpright(PlayerChoiceContext choiceContext, Player player, FeiyapTarotCardBase source)
    {
        var upright = (FeiyapTarotCardBase)player.RunState.CloneCard(source);
        upright.IsReversed = false;
        upright.OrientationInitialized = true;

        var reversed = (FeiyapTarotCardBase)player.RunState.CloneCard(source);
        reversed.IsReversed = true;
        reversed.OrientationInitialized = true;

        var selected = await CardSelectCmd.FromChooseACardScreen(
            choiceContext,
            [upright, reversed],
            player);

        return selected == upright;
    }

    /// <summary>根据 XXI-世界 等效果，解析打出时应使用的逆位标记。</summary>
    public static async Task<bool> ResolveEffectiveReversed(
        PlayerChoiceContext choiceContext,
        FeiyapTarotCardBase card)
    {
        var player = card.Owner;
        if (player == null)
        {
            return card.IsReversed;
        }

        if (HasDualEffect(player))
        {
            return false;
        }

        if (HasFreeChoice(player))
        {
            var upright = await ChooseUpright(choiceContext, player, card);
            return !upright;
        }

        return card.IsReversed;
    }
}
