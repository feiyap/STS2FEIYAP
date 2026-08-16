using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Feiyap.Powers;

/// <summary>
/// XX-审判（逆位）：回合开始时失去 2 点敏捷。
/// </summary>
[RegisterPower]
public sealed class FeiyapJudgementReversedPower : ModPowerTemplate
{
    private const decimal DexterityLoss = 2m;

    public override PowerType Type => PowerType.Debuff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override PowerAssetProfile AssetProfile =>
        FeiyapPowerAssets.ForSharedIcon(nameof(FeiyapJudgementReversedPower), "FeiyapStrengthDownPower");

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player.Creature != Owner)
        {
            return;
        }

        Flash();
        await PowerCmd.Apply<DexterityPower>(
            choiceContext,
            Owner,
            -DexterityLoss,
            Owner,
            null);
    }
}
