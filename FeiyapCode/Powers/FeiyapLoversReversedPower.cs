using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Feiyap.Powers;

/// <summary>
/// VI-恋人（逆位）：添加在目标身上；目标下一次受到伤害时，由自身对目标造成相同伤害。
/// </summary>
[RegisterPower]
public sealed class FeiyapLoversReversedPower : ModPowerTemplate
{
    private bool _resolving;

    public override PowerType Type => PowerType.Debuff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override PowerInstanceType InstanceType => PowerInstanceType.Instanced;

    public override PowerAssetProfile AssetProfile =>
        FeiyapPowerAssets.ForSharedIcon(nameof(FeiyapLoversReversedPower), "FeiyapLoversPower");

    public override async Task BeforeDamageReceived(
        PlayerChoiceContext choiceContext,
        Creature target,
        decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        if (_resolving
            || amount <= 0m
            || target != Owner
            || Applier == null)
        {
            return;
        }

        // 必须先移除再造成镜像伤害，否则会对同一目标再次进入本回调并栈溢出。
        _resolving = true;
        Flash();
        await PowerCmd.Remove(this);
        await FeiyapLoversPowerUtil.DealMirrorDamage(choiceContext, Owner, amount, Applier);
    }
}
