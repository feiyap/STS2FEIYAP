using System.Linq;
using Feiyap.Characters;
using Feiyap.Mechanics;
using Feiyap.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Scaffolding.Content;

namespace Feiyap.Cards.Uncommon;

/// <summary>
/// 无明：造成 6 / 9 点伤害；获得等量于所造成伤害的居合；本次攻击不消耗居合。
/// </summary>
[RegisterCard(typeof(FeiyapCardPool))]
public sealed class FeiyapUncommon7 : FeiyapCardTemplate
{
    protected override HashSet<CardTag> CanonicalTags => new() { FeiyapCardTags.SkipIaidoConsume };

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        FeiyapKeywords.Iaido
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(6, ValueProp.Move)
    ];

    public FeiyapUncommon7()
        : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);

        var attack = await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, cardPlay)
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);

        var damageDealt = attack.Results
            .SelectMany(results => results)
            .Sum(result => result.UnblockedDamage);

        if (damageDealt > 0m)
        {
            await FeiyapIaidoCmd.Gain(
                choiceContext,
                Owner.Creature,
                damageDealt,
                ValueProp.Move,
                this,
                cardPlay);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3m);
    }
}
