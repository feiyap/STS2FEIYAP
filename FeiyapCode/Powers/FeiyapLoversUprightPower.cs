using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Feiyap.Powers;

/// <summary>
/// VI-恋人（正位）：添加在自身；下一次受到伤害时，对目标造成相同伤害。
/// </summary>
[RegisterPower]
public sealed class FeiyapLoversUprightPower : ModPowerTemplate
{
    private const string TargetVarName = "Target";

    private Creature? _markedTarget;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override PowerInstanceType InstanceType => PowerInstanceType.Instanced;

    public override PowerAssetProfile AssetProfile =>
        FeiyapPowerAssets.ForSharedIcon(nameof(FeiyapLoversUprightPower), "FeiyapLoversPower");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new StringVar(TargetVarName)];

    public void SetTarget(Creature target)
    {
        AssertMutable();
        _markedTarget = target;
        ((StringVar)DynamicVars[TargetVarName]).StringValue = target.Name;
    }

    public override async Task BeforeDamageReceived(
        PlayerChoiceContext choiceContext,
        Creature target,
        decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        if (amount <= 0m
            || _markedTarget == null
            || target != Owner
            || dealer == null
            || dealer.Side == Owner.Side
            || !props.IsPoweredAttack())
        {
            return;
        }

        Flash();
        await FeiyapLoversPowerUtil.DealMirrorDamage(choiceContext, _markedTarget, amount, Owner);
        await PowerCmd.Remove(this);
    }
}
