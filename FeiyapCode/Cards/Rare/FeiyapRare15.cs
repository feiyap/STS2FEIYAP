using Feiyap.Characters;
using Feiyap.Mechanics;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Feiyap.Cards.Rare;

/// <summary>
/// 莲生双面：下若干张塔罗牌同时触发正位与逆位效果。
/// </summary>
[RegisterCard(typeof(FeiyapCardPool))]
public sealed class FeiyapRare15 : FeiyapCardTemplate
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("DualPlays", 1m)
    ];

    public FeiyapRare15()
        : base(0, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        FeiyapCombatTracker.Get(Owner).DualTarotPlaysRemaining +=
            (int)DynamicVars["DualPlays"].BaseValue;
        return Task.CompletedTask;
    }

    protected override void OnUpgrade()
    {
        DynamicVars["DualPlays"].UpgradeValueBy(1m);
    }
}
