using System.Linq;
using System.Threading.Tasks;
using Feiyap.Characters;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Scaffolding.Content;

namespace Feiyap.Cards.Ancients;

/// <summary>
/// 衍生牌：星天陨辍。
/// </summary>
[RegisterCard(typeof(FeiyapCardPool))]
public sealed class XingTianYunZhui : FeiyapCardTemplate
{
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Exhaust,
        CardKeyword.Retain
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(1, ValueProp.Move),
        new DynamicVar("EffectBonus", 4m)
    ];

    public XingTianYunZhui()
        : base(2, CardType.Attack, CardRarity.Token, TargetType.AnyEnemy, showInCardLibrary: true)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);

        var effectStacks = CountEffectStacks(cardPlay.Target);
        var totalDamage = DynamicVars.Damage.BaseValue
            + effectStacks * DynamicVars["EffectBonus"].BaseValue;

        await DamageCmd.Attack(totalDamage)
            .FromCard(this)
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["EffectBonus"].UpgradeValueBy(4m);
    }

    private static int CountEffectStacks(Creature target) =>
        target.Powers.Sum(power => power.Amount);
}
