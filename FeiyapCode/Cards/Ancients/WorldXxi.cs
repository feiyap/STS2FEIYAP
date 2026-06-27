using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Feiyap.Characters;
using Feiyap.Mechanics;
using Feiyap.Powers;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Saves.Runs;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Scaffolding.Characters;
using STS2RitsuLib.Scaffolding.Content;

namespace Feiyap.Cards.Ancients;

/// <summary>
/// 先古卡：XXI-世界。
/// </summary>
[RegisterCard(typeof(FeiyapCardPool))]
public sealed class WorldXxi : FeiyapCardTemplate
{
    private bool _isReversed;
    private bool _orientationInitialized;

    public override IEnumerable<CardKeyword> CanonicalKeywords
    {
        get
        {
            var keywords = new List<CardKeyword>
            {
                FeiyapKeywords.TarotUpright,
                FeiyapKeywords.TarotReversed
            };

            if (!IsUpgraded)
            {
                keywords.Add(CardKeyword.Exhaust);
            }

            return keywords;
        }
    }

    [SavedProperty]
    public bool IsReversed
    {
        get => _isReversed;
        set
        {
            AssertMutable();
            _isReversed = value;
        }
    }

    [SavedProperty]
    public bool OrientationInitialized
    {
        get => _orientationInitialized;
        set
        {
            AssertMutable();
            _orientationInitialized = value;
        }
    }

    public WorldXxi()
        : base(1, CardType.Skill, CardRarity.Ancient, TargetType.Self, showInCardLibrary: true)
    {
    }

    public override bool TryModifyEnergyCostInCombatLate(
        CardModel card,
        decimal originalCost,
        out decimal modifiedCost)
    {
        modifiedCost = originalCost;
        if (card != this || !IsReversed)
        {
            return false;
        }

        modifiedCost = originalCost + 2m;
        return true;
    }

    public override Task BeforeSideTurnStart(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        EnsureOrientationInitialized();
        return Task.CompletedTask;
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        EnsureOrientationInitialized();

        var pool = FeiyapTarotRegistry.CreateSelectionPool(Owner, this).ToList();
        if (pool.Count == 0)
        {
            return;
        }

        var selectCount = IsReversed ? 3 : 1;
        selectCount = Math.Min(selectCount, pool.Count);

        var prompt = new LocString("cards", "FEIYAP_CARD_WORLD_XXI.selectionPrompt");
        var prefs = new CardSelectorPrefs(prompt, selectCount, selectCount)
        {
            Cancelable = false,
            RequireManualConfirmation = true
        };

        var selected = await CardSelectCmd.FromSimpleGrid(choiceContext, pool, Owner, prefs);
        var generated = selected.ToList();
        if (generated.Count == 0)
        {
            return;
        }

        foreach (var card in generated)
        {
            if (card is Feiyap.Cards.Tarot.FeiyapTarotCardBase tarot)
            {
                tarot.RollOrientation(Owner.RunState.Rng.Niche);
            }
        }

        await CardPileCmd.Add(generated, PileType.Hand);

        var freePlay = Owner.Creature.FindPower<FeiyapTarotFreePlayPower>();
        if (freePlay == null)
        {
            await PowerCmd.Apply(
                choiceContext,
                ModelDb.Power<FeiyapTarotFreePlayPower>().ToMutable(),
                Owner.Creature,
                1m,
                Owner.Creature,
                this);
        }
    }

    protected override void OnUpgrade()
    {
        CardCmd.RemoveKeyword(this, CardKeyword.Exhaust);
    }

    private void EnsureOrientationInitialized()
    {
        if (OrientationInitialized || !IsMutable)
        {
            return;
        }

        RollOrientation(Owner.RunState.Rng.Niche);
        OrientationInitialized = true;
    }

    private void RollOrientation(Rng rng)
    {
        IsReversed = rng.NextBool();
    }
}
