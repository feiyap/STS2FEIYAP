using System.Linq;
using Feiyap.Characters;
using Feiyap.Mechanics;
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
/// 天津四：星座，造成 9 / 12 点伤害，抽 1 张随机星座牌。
/// </summary>
[RegisterCard(typeof(FeiyapCardPool))]
public sealed class FeiyapCommon6 : ModCardTemplate
{
    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/{GetType().Name}.png");

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        FeiyapKeywords.Constellation
    ];

    protected override HashSet<CardTag> CanonicalTags => new()
    {
        FeiyapCardTags.Constellation
    };

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(9, ValueProp.Move),
        new CardsVar(1)
    ];

    public FeiyapCommon6()
        : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);

        await FeiyapConstellationCmd.OnPlayed(choiceContext, Owner);

        var allCards = Owner.PlayerCombatState?.AllCards;
        if (allCards == null)
        {
            return;
        }

        var candidates = allCards
            .Where(c => FeiyapCardTags.HasConstellation(c) && c != this)
            .Where(c => c.Pile?.Type is PileType.Draw or PileType.Discard)
            .ToList();

        var drawn = Owner.RunState.Rng.CombatCardSelection.NextItem(candidates);
        if (drawn != null)
        {
            await CardPileCmd.Add(drawn, PileType.Hand);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3m);
    }
}
