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
/// 破绽：每层使受到的伤害提升 10%；受到攻击伤害时叠加 1 层；达到 5 层后于回合结束时移除。
/// </summary>
[RegisterPower]
public sealed class FeiyapPozhanPower : ModPowerTemplate
{
    private const decimal DamageBonusPerStack = 0.10m;
    private const decimal ClearThreshold = 5m;

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

    public override async Task AfterDamageReceived(
        PlayerChoiceContext choiceContext,
        Creature target,
        DamageResult result,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        if (target != Owner || Amount <= 0m || !props.IsPoweredAttack())
        {
            return;
        }

        Flash();
        await PowerCmd.ModifyAmount(choiceContext, this, 1m, dealer, cardSource);
    }

    public override async Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (!participants.Contains(Owner) || Amount < ClearThreshold)
        {
            return;
        }

        Flash();
        await PowerCmd.Remove(this);
    }
}
