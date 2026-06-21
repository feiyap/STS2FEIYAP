using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Feiyap.Characters;
using Feiyap.Mechanics;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Feiyap.Cards.Ancients;

/// <summary>
/// 先古卡：无声星陨。
/// </summary>
[RegisterCard(typeof(FeiyapCardPool))]
public sealed class WuShengXingYun : ModCardTemplate
{
    public WuShengXingYun()
        : base(1, CardType.Attack, CardRarity.Ancient, TargetType.AnyEnemy, showInCardLibrary: false)
    {
    }

    protected override HashSet<CardTag> CanonicalTags => new() { FeiyapCardTags.InternalBurn };

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(14, ValueProp.Move)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);
    }
}
