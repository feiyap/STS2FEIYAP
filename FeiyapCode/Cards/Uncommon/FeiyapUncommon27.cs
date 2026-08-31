using Feiyap.Cards.Tarot;
using Feiyap.Characters;
using Feiyap.Mechanics;
using Feiyap.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Feiyap.Cards.Uncommon;

/// <summary>
/// IX-隐者：正位获得活力，逆位获得残心。
/// </summary>
[RegisterCard(typeof(FeiyapCardPool))]
public sealed class FeiyapUncommon27 : FeiyapTarotCardBase
{
    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromPower<VigorPower>(),
        HoverTipFactory.FromPower<FeiyapZanxinPower>()
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<VigorPower>(7m),
        new PowerVar<FeiyapZanxinPower>(7m)
    ];

    public FeiyapUncommon27()
        : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
        RegisterTarotFactory(player => player.RunState.CreateCard<FeiyapUncommon27>(player));
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        EnsureOrientationInitialized();

        await RunTarotBranches(
            choiceContext,
            async () =>
            {
                await PowerCmd.Apply<VigorPower>(
                    choiceContext,
                    Owner.Creature,
                    DynamicVars["VigorPower"].BaseValue,
                    Owner.Creature,
                    this);
            },
            async () =>
            {
                await FeiyapZanxinCmd.Gain(
                    choiceContext,
                    Owner.Creature,
                    DynamicVars["FeiyapZanxinPower"].BaseValue,
                    this);
            });
    }

    protected override void OnUpgrade()
    {
        DynamicVars["VigorPower"].UpgradeValueBy(3m);
        DynamicVars["FeiyapZanxinPower"].UpgradeValueBy(3m);
    }
}
