using Feiyap.Characters;
using Feiyap.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Feiyap.Cards.Rare;

/// <summary>
/// 天下五剑：造成 5 点伤害 5 次；升级后力量与活力获得 2 倍效果。
/// </summary>
[RegisterCard(typeof(FeiyapCardPool))]
public sealed class FeiyapRare1 : FeiyapCardTemplate
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(5, ValueProp.Move),
        new RepeatVar(5),
        new PowerVar<FeiyapTenkaFiveSwordsBoostPower>(2m)
    ];

    public FeiyapRare1()
        : base(3, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);

        if (IsUpgraded)
        {
            await PowerCmd.Apply(
                choiceContext,
                ModelDb.Power<FeiyapTenkaFiveSwordsBoostPower>().ToMutable(),
                Owner.Creature,
                DynamicVars["FeiyapTenkaFiveSwordsBoostPower"].BaseValue,
                Owner.Creature,
                this);
        }

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .WithHitCount(DynamicVars.Repeat.IntValue)
            .FromCard(this)
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);

        if (IsUpgraded)
        {
            var boost = Owner.Creature.GetPower<FeiyapTenkaFiveSwordsBoostPower>();
            if (boost != null)
            {
                await PowerCmd.Remove(boost);
            }
        }
    }

    protected override void OnUpgrade()
    {
        // 升级效果由临时倍率能力承担。
    }
}
