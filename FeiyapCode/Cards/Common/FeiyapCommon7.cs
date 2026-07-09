using System.Linq;
using Feiyap.Characters;
using Feiyap.Cards.Tarot;
using Feiyap.Mechanics;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Feiyap.Cards.Common;

/// <summary>
/// IV-皇帝：造成 8 / 11 点伤害；正位抽 1 张随机塔罗牌，逆位抽 1 张随机非塔罗牌。
/// </summary>
[RegisterCard(typeof(FeiyapCardPool))]
public sealed class FeiyapCommon7 : FeiyapTarotCardBase
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(8, ValueProp.Move)
    ];

    public FeiyapCommon7()
        : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
    {
        RegisterTarotFactory(player => player.RunState.CreateCard<FeiyapCommon7>(player));
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        EnsureOrientationInitialized();
        ArgumentNullException.ThrowIfNull(cardPlay.Target);

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, cardPlay)
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);

        await RunTarotBranches(
            choiceContext,
            () => DrawRandomTarotCard(choiceContext),
            () => DrawRandomNonTarotCard(choiceContext));
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3m);
    }

    private async Task DrawRandomTarotCard(PlayerChoiceContext choiceContext)
    {
        var allCards = Owner.PlayerCombatState?.AllCards;
        var candidates = allCards?
            .Where(c => FeiyapCardTags.HasTarot(c) && c != this)
            .Where(c => c.Pile?.Type is PileType.Draw or PileType.Discard)
            .ToList();

        var card = Owner.RunState.Rng.CombatCardSelection.NextItem(candidates ?? []);
        if (card == null)
        {
            var pool = FeiyapTarotRegistry.CreateSelectionPool(Owner, this).ToList();
            var created = Owner.RunState.Rng.CombatCardSelection.NextItem(pool);
            if (created == null)
            {
                return;
            }

            card = Owner.RunState.CloneCard(created);
        }

        await CardPileCmd.Add(card, PileType.Hand);
    }

    private async Task DrawRandomNonTarotCard(PlayerChoiceContext choiceContext)
    {
        var drawPile = PileType.Draw.GetPile(Owner);
        var card = Owner.RunState.Rng.CombatCardSelection.NextItem(
            drawPile.Cards.Where(c => !FeiyapCardTags.HasTarot(c)));

        if (card != null)
        {
            await CardPileCmd.Add(card, PileType.Hand);
        }
    }
}
