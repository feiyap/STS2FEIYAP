using Feiyap.Characters;
using Feiyap.Cards.Tarot;
using Feiyap.Mechanics;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Feiyap.Cards.Uncommon;

/// <summary>
/// II-女祭司：获得格挡；正位获得活力，逆位抽牌。
/// </summary>
[RegisterCard(typeof(FeiyapCardPool))]
public sealed class FeiyapUncommon23 : FeiyapTarotCardBase
{
    public override bool GainsBlock => true;

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromPower<VigorPower>()
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(12m, ValueProp.Move),
        new PowerVar<VigorPower>(2m),
        new CardsVar(1)
    ];

    public FeiyapUncommon23()
        : base(2, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
        RegisterTarotFactory(player => player.RunState.CreateCard<FeiyapUncommon23>(player));
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        EnsureOrientationInitialized();
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);

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
            () => CardPileCmd.Draw(choiceContext, DynamicVars.Cards.IntValue, Owner));
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(4m);
        DynamicVars["VigorPower"].UpgradeValueBy(1m);
        DynamicVars.Cards.UpgradeValueBy(1m);
    }
}
