using System.Linq;
using Feiyap.Characters;
using Feiyap.Cards.Tarot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Feiyap.Cards.Common;

/// <summary>
/// XVII-星星：塔罗技能牌，正位抽随机攻击牌，逆位抽随机技能牌。
/// </summary>
[RegisterCard(typeof(FeiyapCardPool))]
public sealed class FeiyapCommon20 : FeiyapTarotCardBase
{

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CardsVar(2)
    ];

    public FeiyapCommon20()
        : base(1, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
        RegisterTarotFactory(player => player.RunState.CreateCard<FeiyapCommon20>(player));
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        EnsureOrientationInitialized();

        await RunTarotBranches(
            choiceContext,
            async () =>
            {
                for (var i = 0; i < DynamicVars.Cards.IntValue; i++)
                {
                    await AddRandomTypedCardToHand(CardType.Attack);
                }
            },
            async () =>
            {
                for (var i = 0; i < DynamicVars.Cards.IntValue; i++)
                {
                    await AddRandomTypedCardToHand(CardType.Skill);
                }
            });
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }

    private async Task AddRandomTypedCardToHand(CardType type)
    {
        var candidates = Owner.PlayerCombatState?.AllCards
            .Where(c => c.Type == type)
            .Where(c => c.Pile?.Type is PileType.Draw or PileType.Discard)
            .ToList();

        var card = Owner.RunState.Rng.CombatCardSelection.NextItem(candidates ?? []);
        if (card != null)
        {
            await CardPileCmd.Add(card, PileType.Hand);
        }
    }
}
