using Feiyap.Characters;
using Feiyap.Cards.Tarot;
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
/// III-皇后：正位获得居合 2 次，逆位获得格挡 2 次。
/// </summary>
[RegisterCard(typeof(FeiyapCardPool))]
public sealed class FeiyapUncommon25 : FeiyapTarotCardBase
{
    public override bool GainsBlock => true;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [FeiyapKeywords.Iaido];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new IaidoVar(4m, ValueProp.Move),
        new BlockVar(4m, ValueProp.Move),
        new RepeatVar(2)
    ];

    public FeiyapUncommon25()
        : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
        RegisterTarotFactory(player => player.RunState.CreateCard<FeiyapUncommon25>(player));
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        EnsureOrientationInitialized();
        var repeats = DynamicVars.Repeat.IntValue;

        await RunTarotBranches(
            choiceContext,
            async () =>
            {
                for (var i = 0; i < repeats; i++)
                {
                    await FeiyapIaidoCmd.Gain(
                        choiceContext,
                        Owner.Creature,
                        DynamicVars[IaidoVar.DefaultName].BaseValue,
                        ValueProp.Move,
                        this,
                        cardPlay);
                }
            },
            async () =>
            {
                for (var i = 0; i < repeats; i++)
                {
                    await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
                }
            });
    }

    protected override void OnUpgrade()
    {
        DynamicVars[IaidoVar.DefaultName].UpgradeValueBy(2m);
        DynamicVars.Block.UpgradeValueBy(2m);
    }
}
