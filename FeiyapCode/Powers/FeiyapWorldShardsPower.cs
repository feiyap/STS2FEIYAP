using Feiyap.Mechanics;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Feiyap.Powers;

/// <summary>
/// 于万千碎裂的世界破片：回合开始时获得多种属性。
/// </summary>
[RegisterPower]
public sealed class FeiyapWorldShardsPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override PowerAssetProfile AssetProfile =>
        FeiyapPowerAssets.ForSharedIcon(nameof(FeiyapWorldShardsPower), nameof(FeiyapKachuuFuugetsuPower));

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player.Creature != Owner)
        {
            return;
        }

        Flash();
        await PowerCmd.Apply<StrengthPower>(choiceContext, Owner, 1m, Owner, null);
        await PowerCmd.Apply<DexterityPower>(choiceContext, Owner, 1m, Owner, null);
        await PowerCmd.Apply<FocusPower>(choiceContext, Owner, 1m, Owner, null);
        await CreatureCmd.GainBlock(Owner, 1m, ValueProp.Move, null);
        await FeiyapIaidoCmd.Gain(choiceContext, Owner, 1m, ValueProp.Move, null, null);
        await PowerCmd.Apply<VigorPower>(choiceContext, Owner, 1m, Owner, null);
        await FeiyapZanxinCmd.Gain(choiceContext, Owner, 1m, null);
        await PowerCmd.Apply<ThornsPower>(choiceContext, Owner, 1m, Owner, null);
        await PowerCmd.Apply<PlatingPower>(choiceContext, Owner, 1m, Owner, null);
    }
}
