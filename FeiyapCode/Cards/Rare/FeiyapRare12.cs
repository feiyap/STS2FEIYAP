using System.Linq;
using Feiyap.Characters;
using Feiyap.Cards.Uncommon;
using Feiyap.Mechanics;
using Feiyap.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Feiyap.Cards.Rare;

/// <summary>
/// 绯神乐：下一张攻击/技能不消耗居合并额外伤害；取鬼镰月到手并解锁神座屠。
/// </summary>
[RegisterCard(typeof(FeiyapCardPool))]
public sealed class FeiyapRare12 : FeiyapCardTemplate
{
    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromPower<FeiyapScarletKaguraPower>()
    ];

    public FeiyapRare12()
        : base(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply(
            choiceContext,
            ModelDb.Power<FeiyapScarletKaguraPower>().ToMutable(),
            Owner.Creature,
            1m,
            Owner.Creature,
            this);

        await FetchCardToHand<FeiyapUncommon22>();
        FeiyapCombatTracker.Get(Owner).ShinzatoUnlockedThisTurn = true;
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }

    private async Task FetchCardToHand<T>() where T : CardModel
    {
        var candidates = Owner.PlayerCombatState?.AllCards
            .Where(c => c is T)
            .Where(c => c.Pile?.Type is PileType.Draw or PileType.Discard)
            .ToList();

        var card = Owner.RunState.Rng.CombatCardSelection.NextItem(candidates ?? []);
        if (card != null)
        {
            await CardPileCmd.Add(card, PileType.Hand);
        }
    }
}
