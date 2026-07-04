using Feiyap.Characters;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Feiyap.Cards.Rare;

/// <summary>
/// XIX-太阳：群体攻击；握牌时打出攻击牌后追加伤害并变为 XVIII-月亮。
/// </summary>
[RegisterCard(typeof(FeiyapCardPool))]
public sealed class FeiyapRare6 : FeiyapCardTemplate
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(12, ValueProp.Move),
        new DynamicVar("TriggerDamage", 9m)
    ];

    public FeiyapRare6()
        : base(1, CardType.Attack, CardRarity.Rare, TargetType.AllEnemies)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .TargetingAllOpponents(CombatState!)
            .Execute(choiceContext);
    }

    public override async Task AfterCardPlayedLate(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner != Owner
            || cardPlay.Card.Type != CardType.Attack
            || cardPlay.Card == this
            || Pile?.Type != PileType.Hand
            || CombatState == null)
        {
            return;
        }

        await DamageCmd.Attack(DynamicVars["TriggerDamage"].BaseValue)
            .FromCard(this)
            .TargetingAllOpponents(CombatState)
            .Execute(choiceContext);

        var moon = Owner.RunState.CreateCard<FeiyapRare12>(Owner);
        if (IsUpgraded)
        {
            CardCmd.Upgrade(moon);
        }

        await CardCmd.Transform(this, moon);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(6m);
        DynamicVars["TriggerDamage"].UpgradeValueBy(3m);
    }
}
