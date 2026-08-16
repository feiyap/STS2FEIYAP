using System.Linq;
using Feiyap.Characters;
using Feiyap.Mechanics;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Scaffolding.Content;

namespace Feiyap.Cards.Uncommon;

/// <summary>
/// 孤燕之瞥：消耗任意数量手牌；状态牌失血获残心，非状态牌失血抽牌。
/// </summary>
[RegisterCard(typeof(FeiyapCardPool))]
public sealed class FeiyapUncommon17 : FeiyapCardTemplate
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [FeiyapKeywords.Zanxin];

    public FeiyapUncommon17()
        : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var handCount = Owner.PlayerCombatState?.Hand.Cards.Count ?? 0;
        if (handCount <= 0)
        {
            return;
        }

        var prefs = new CardSelectorPrefs(SelectionScreenPrompt, 0, handCount)
        {
            RequireManualConfirmation = true
        };

        var selected = (await CardSelectCmd.FromHand(
            choiceContext,
            Owner,
            prefs,
            filter: null,
            source: this)).ToList();

        foreach (var card in selected)
        {
            var isStatus = card.Type == CardType.Status;
            await CardCmd.Exhaust(choiceContext, card);
            var hpLoss = 1;
            await CreatureCmd.Damage(
                choiceContext,
                Owner.Creature,
                hpLoss,
                ValueProp.Unblockable | ValueProp.Unpowered,
                null,
                this,
                null);

            if (isStatus)
            {
                await FeiyapZanxinCmd.Gain(choiceContext, Owner.Creature, 1m, this);
            }
            else
            {
                await CardPileCmd.Draw(choiceContext, 1, Owner);
            }
        }
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
