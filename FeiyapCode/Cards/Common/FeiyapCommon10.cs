using Feiyap.Characters;
using Feiyap.Mechanics;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Feiyap.Cards.Common;

/// <summary>
/// 速纳术：将当前所有格挡转化为居合；升级后再获得 3 点居合。
/// </summary>
[RegisterCard(typeof(FeiyapCardPool))]
public sealed class FeiyapCommon10 : FeiyapCardTemplate
{
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        FeiyapKeywords.Iaido
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new IaidoVar(3m, ValueProp.Move)
    ];

    public FeiyapCommon10()
        : base(0, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var creature = Owner.Creature;
        var block = creature.Block;
        if (block > 0m)
        {
            await CreatureCmd.LoseBlock(choiceContext, creature, block, creature);
            await FeiyapIaidoCmd.Gain(
                choiceContext,
                creature,
                block,
                ValueProp.Unpowered,
                cardSource: null,
                cardPlay: null);
        }

        if (IsUpgraded)
        {
            await FeiyapIaidoCmd.Gain(
                choiceContext,
                creature,
                DynamicVars[IaidoVar.DefaultName].BaseValue,
                ValueProp.Move,
                this,
                cardPlay);
        }
    }
}
