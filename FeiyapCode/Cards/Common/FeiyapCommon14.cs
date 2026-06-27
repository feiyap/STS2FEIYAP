using System.Linq;
using Feiyap.Characters;
using Feiyap.Mechanics;
using Feiyap.Powers;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Scaffolding.Content;

namespace Feiyap.Cards.Common;

/// <summary>
/// 踏影步：丢弃 1 张牌，获得 8 / 11 点居合。
/// </summary>
[RegisterCard(typeof(FeiyapCardPool))]
public sealed class FeiyapCommon14 : FeiyapCardTemplate
{

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        FeiyapKeywords.Iaido
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<FeiyapIaidoPower>(8m)
    ];

    public FeiyapCommon14()
        : base(1, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var toDiscard = await CardSelectCmd.FromHandForDiscard(
            choiceContext,
            Owner,
            new CardSelectorPrefs(CardSelectorPrefs.DiscardSelectionPrompt, 1),
            null,
            this);

        var card = toDiscard.FirstOrDefault();
        if (card != null)
        {
            await CardCmd.Discard(choiceContext, card);
        }

        await FeiyapIaidoCmd.Gain(
            choiceContext,
            Owner.Creature,
            DynamicVars["FeiyapIaidoPower"].BaseValue,
            ValueProp.Move,
            this,
            cardPlay);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["FeiyapIaidoPower"].UpgradeValueBy(3m);
    }
}
