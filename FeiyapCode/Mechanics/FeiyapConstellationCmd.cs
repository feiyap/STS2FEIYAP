using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Feiyap.Mechanics;

/// <summary>
/// 星座牌打出后的共通效果：获得原版储君星星。
/// </summary>
public static class FeiyapConstellationCmd
{
    public static Task OnPlayed(PlayerChoiceContext choiceContext, Player player, decimal stars = 1m) =>
        PlayerCmd.GainStars(stars, player);
}
