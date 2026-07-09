using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Feiyap.Powers;

/// <summary>
/// 剑心犹在：上回合未失去生命时，回合开始获得敏捷。
/// </summary>
[RegisterPower]
public sealed class FeiyapSwordHeartRemainsPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => FeiyapPowerAssets.For(nameof(FeiyapSwordHeartRemainsPower));

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player.Creature != Owner || Amount <= 0)
        {
            return;
        }

        if (LostHpLastPlayerTurn(player))
        {
            return;
        }

        Flash();
        await PowerCmd.Apply<DexterityPower>(
            choiceContext,
            Owner,
            Amount,
            Owner,
            null);
    }

    /// <summary>
    /// 与原版 EmotionChip 一致：查询战斗历史判断玩家上回合是否受过未格挡伤害。
    /// </summary>
    private static bool LostHpLastPlayerTurn(Player player) =>
        CombatManager.Instance.History.Entries
            .OfType<DamageReceivedEntry>()
            .Any(entry =>
                entry.Receiver == player.Creature
                && !entry.Result.WasFullyBlocked
                && entry.HappenedLastPlayerTurn(player));
}
