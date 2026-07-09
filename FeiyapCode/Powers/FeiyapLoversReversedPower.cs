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
/// VI-恋人（逆位）：添加在目标身上；目标下一次受到伤害时，由自身对目标造成相同伤害。
/// </summary>
[RegisterPower]
public sealed class FeiyapLoversReversedPower : ModPowerTemplate
{
    private const string TargetVarName = "Target";

    public override PowerType Type => PowerType.Debuff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override PowerInstanceType InstanceType => PowerInstanceType.Instanced;

    public override PowerAssetProfile AssetProfile =>
        FeiyapPowerAssets.ForSharedIcon(nameof(FeiyapLoversReversedPower), "FeiyapLoversPower");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new StringVar(TargetVarName)];

    public override Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        ((StringVar)DynamicVars[TargetVarName]).StringValue = Owner.Name;
        return Task.CompletedTask;
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
            || target != Owner
            || Applier == null)
        {
            return;
        }

        Flash();
        await FeiyapLoversPowerUtil.DealMirrorDamage(choiceContext, Owner, amount, Applier);
        await PowerCmd.Remove(this);
    }
}
