using System.Linq;
using Feiyap.Cards.Quest;
using Feiyap.Characters;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Feiyap.Cards.Uncommon;

/// <summary>
/// 明镜止水：消耗所有状态牌、诅咒牌和任务牌；每消耗 1 张，获得 1 / 2 点活力。消耗。
/// </summary>
[RegisterCard(typeof(FeiyapCardPool))]
public sealed class FeiyapUncommon18 : FeiyapCardTemplate
{

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Exhaust
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromPower<VigorPower>()
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<VigorPower>(1m)
    ];

    public FeiyapUncommon18()
        : base(2, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var toExhaust = Owner.PlayerCombatState?.AllCards
            .Where(IsNegativeCard)
            .Where(c => c.Pile?.Type != PileType.Exhaust)
            .ToList() ?? [];

        foreach (var card in toExhaust)
        {
            await CardCmd.Exhaust(choiceContext, card);
        }

        if (toExhaust.Count > 0)
        {
            var vigor = DynamicVars["VigorPower"].BaseValue * toExhaust.Count;
            await PowerCmd.Apply<VigorPower>(
                choiceContext,
                Owner.Creature,
                vigor,
                Owner.Creature,
                this);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["VigorPower"].UpgradeValueBy(1m);
    }

    private static bool IsNegativeCard(CardModel card) =>
        card.Type is CardType.Status or CardType.Curse or CardType.Quest
        || card is FeiyapQuestCardBase;
}
