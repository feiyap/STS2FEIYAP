using System.Linq;
using Feiyap.Characters;
using Feiyap.Cards.Rare;
using Feiyap.Mechanics;
using Feiyap.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Scaffolding.Content;

namespace Feiyap.Cards.Uncommon;

/// <summary>
/// 鬼镰月：获得居合；本回合居合伤害提升；从抽牌/弃牌堆取神座屠到手。
/// </summary>
[RegisterCard(typeof(FeiyapCardPool))]
public sealed class FeiyapUncommon22 : FeiyapCardTemplate
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [FeiyapKeywords.Iaido];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromPower<FeiyapGuilianMoonPower>()
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new IaidoVar(6m, ValueProp.Move)
    ];

    public FeiyapUncommon22()
        : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await FeiyapIaidoCmd.Gain(
            choiceContext,
            Owner.Creature,
            DynamicVars[IaidoVar.DefaultName].BaseValue,
            ValueProp.Move,
            this,
            cardPlay);

        await PowerCmd.Apply(
            choiceContext,
            ModelDb.Power<FeiyapGuilianMoonPower>().ToMutable(),
            Owner.Creature,
            1m,
            Owner.Creature,
            this);

        await FetchCardToHand<FeiyapRare5>();
    }

    protected override void OnUpgrade()
    {
        DynamicVars[IaidoVar.DefaultName].UpgradeValueBy(3m);
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
