using Feiyap.Characters;
using Feiyap.Cards.Tarot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Feiyap.Cards.Uncommon;

/// <summary>
/// I-魔术师：正位抽牌，逆位获得耗能。
/// </summary>
[RegisterCard(typeof(FeiyapCardPool))]
public sealed class FeiyapUncommon20 : FeiyapTarotCardBase
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CardsVar(3),
        new EnergyVar(3)
    ];

    public FeiyapUncommon20()
        : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
        RegisterTarotFactory(player => player.RunState.CreateCard<FeiyapUncommon20>(player));
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        EnsureOrientationInitialized();

        await RunTarotBranches(
            choiceContext,
            () => CardPileCmd.Draw(choiceContext, DynamicVars.Cards.IntValue, Owner),
            () => PlayerCmd.GainEnergy(DynamicVars.Energy.BaseValue, Owner));
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Cards.UpgradeValueBy(1m);
        DynamicVars.Energy.UpgradeValueBy(1m);
    }
}
