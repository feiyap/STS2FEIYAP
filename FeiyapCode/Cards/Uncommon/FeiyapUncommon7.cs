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
/// 无明：仅能在有居合时打出；消耗所有居合并造成多段伤害与额外伤害。
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
        new DamageVar(5, ValueProp.Move),
        new RepeatVar(2)
    ];

    protected override bool IsPlayable =>
        Owner.Creature.GetPowerAmount<FeiyapIaidoPower>() > 0;

    protected override bool ShouldGlowGoldInternal => IsPlayable;

    public FeiyapUncommon7()
        : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);

        var iaido = Owner.Creature.GetPowerAmount<FeiyapIaidoPower>();
        await FeiyapIaidoCmd.ClearAll(choiceContext, Owner.Creature);

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .WithHitCount(DynamicVars.Repeat.IntValue)
            .FromCard(this, cardPlay)
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);

        if (iaido > 0)
        {
            await CreatureCmd.Damage(
                choiceContext,
                cardPlay.Target,
                iaido,
                ValueProp.Move,
                Owner.Creature,
                this,
                cardPlay);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2m);
    }
}
