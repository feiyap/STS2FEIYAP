using Feiyap.Characters;
using Feiyap.Mechanics;
using Feiyap.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Scaffolding.Content;

namespace Feiyap.Cards.Common;

/// <summary>
/// 河鼓二：星座，造成 4 / 7 点伤害，给予 3 / 4 层体内灼烧。
/// </summary>
[RegisterCard(typeof(FeiyapCardPool))]
public sealed class FeiyapCommon8 : FeiyapCardTemplate
{

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        FeiyapKeywords.Constellation,
        FeiyapKeywords.InternalBurn
    ];

    protected override HashSet<CardTag> CanonicalTags => new()
    {
        FeiyapCardTags.Constellation,
        FeiyapCardTags.InternalBurn
    };

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(4, ValueProp.Move),
        new PowerVar<FeiyapInternalBurnPower>(3m)
    ];

    public FeiyapCommon8()
        : base(0, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);

        await PowerCmd.Apply(
            choiceContext,
            ModelDb.Power<FeiyapInternalBurnPower>().ToMutable(),
            cardPlay.Target,
            DynamicVars["FeiyapInternalBurnPower"].BaseValue,
            Owner.Creature,
            this);

        await FeiyapConstellationCmd.OnPlayed(choiceContext, Owner);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3m);
        DynamicVars["FeiyapInternalBurnPower"].UpgradeValueBy(1m);
    }
}
