using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Feiyap.Powers;

/// <summary>
/// 破绽：每层使从攻击中受到的伤害增加 10%；回合结束时层数减半。
/// </summary>
[RegisterPower]
public sealed class FeiyapPozhanPower : ModPowerTemplate
{
    private const decimal DamageBonusPerStack = 0.10m;

    public override PowerType Type => PowerType.Debuff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile =>
        FeiyapPowerAssets.ForSharedIcon(nameof(FeiyapPozhanPower), "FeiyapSwordSaintHeartPower");

    public override decimal ModifyDamageMultiplicative(
        Creature? target,
        decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource,
        CardPlay? cardPlay)
    {
        if (target != Owner || Amount <= 0m || !props.IsPoweredAttack())
        {
            return 1m;
        }

        return 1m + Amount * DamageBonusPerStack;
    }

    public override async Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (!participants.Contains(Owner) || Amount <= 0m)
        {
            return;
        }

        Flash();
        var halved = Math.Floor(Amount / 2m);
        var delta = halved - Amount;
        if (delta >= 0m)
        {
            return;
        }

        await PowerCmd.ModifyAmount(choiceContext, this, delta, null, null);
    }
}
