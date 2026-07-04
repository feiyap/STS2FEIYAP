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

namespace Feiyap.Cards.Uncommon;

/// <summary>
/// 血溅五步：造成 14 点伤害；活力在该牌上发挥 3 / 5 倍效果。
/// </summary>
[RegisterCard(typeof(FeiyapCardPool))]
public sealed class FeiyapUncommon5 : FeiyapCardTemplate
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(14, ValueProp.Move),
        new PowerVar<FeiyapBloodSplashVigorPower>(3m)
    ];

    public FeiyapUncommon5()
        : base(2, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);

        await PowerCmd.Apply(
            choiceContext,
            ModelDb.Power<FeiyapBloodSplashVigorPower>().ToMutable(),
            Owner.Creature,
            DynamicVars["FeiyapBloodSplashVigorPower"].BaseValue,
            Owner.Creature,
            this);

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);

        var boost = Owner.Creature.GetPower<FeiyapBloodSplashVigorPower>();
        if (boost != null)
        {
            await PowerCmd.Remove(boost);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["FeiyapBloodSplashVigorPower"].UpgradeValueBy(2m);
    }
}
