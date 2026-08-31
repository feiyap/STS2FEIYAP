using System.Linq;
using Feiyap.Characters;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Feiyap.Cards.Uncommon;

/// <summary>
/// 叶隐：消耗任意数量手牌，获得等量能量。
/// </summary>
[RegisterCard(typeof(FeiyapCardPool))]
public sealed class FeiyapUncommon19 : FeiyapCardTemplate
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    public FeiyapUncommon19()
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
            await CardCmd.Exhaust(choiceContext, card);
        }

        if (selected.Count > 0)
        {
            await PlayerCmd.GainEnergy(selected.Count, Owner);
        }
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
