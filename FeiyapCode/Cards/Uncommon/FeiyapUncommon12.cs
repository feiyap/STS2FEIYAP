using System.Linq;
using Feiyap.Cards.Quest;
using Feiyap.Characters;
using Feiyap.Mechanics;
using Feiyap.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Scaffolding.Content;

namespace Feiyap.Cards.Uncommon;

/// <summary>
/// 明镜止水：消耗所有玩家的状态牌、诅咒牌和任务牌；每消耗 1 张获得居合强化。消耗。
/// </summary>
[RegisterCard(typeof(FeiyapCardPool))]
public sealed class FeiyapUncommon12 : FeiyapCardTemplate
{
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Exhaust,
        FeiyapKeywords.IaidoEnhance
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<FeiyapIaidoEnhancePower>(1m)
    ];

    public FeiyapUncommon12()
        : base(2, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var toExhaust = CombatState?.Players
            .SelectMany(p => p.PlayerCombatState?.AllCards ?? [])
            .Where(IsNegativeCard)
            .Where(c => c.Pile?.Type != PileType.Exhaust)
            .Distinct()
            .ToList() ?? [];

        foreach (var card in toExhaust)
        {
            await CardCmd.Exhaust(choiceContext, card);
        }

        if (toExhaust.Count > 0)
        {
            var enhance = DynamicVars["FeiyapIaidoEnhancePower"].BaseValue * toExhaust.Count;
            await PowerCmd.Apply<FeiyapIaidoEnhancePower>(
                choiceContext,
                Owner.Creature,
                enhance,
                Owner.Creature,
                this);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["FeiyapIaidoEnhancePower"].UpgradeValueBy(1m);
    }

    private static bool IsNegativeCard(CardModel card) =>
        card.Type is CardType.Status or CardType.Curse or CardType.Quest
        || card is FeiyapQuestCardBase;
}
