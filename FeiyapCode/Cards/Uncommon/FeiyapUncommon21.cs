using Feiyap.Characters;
using Feiyap.Mechanics;
using Feiyap.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Scaffolding.Content;

namespace Feiyap.Cards.Uncommon;

/// <summary>
/// 天五月：保留；本回合内居合对所有敌人造成伤害。每次获得居合，这张牌的耗能减少 1。
/// </summary>
[RegisterCard(typeof(FeiyapCardPool))]
public sealed class FeiyapUncommon21 : FeiyapCardTemplate
{

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Retain,
        FeiyapKeywords.Iaido
    ];

    public FeiyapUncommon21()
        : base(5, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    public void OnIaidoGained()
    {
        EnergyCost.AddThisCombat(-1);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply(
            choiceContext,
            ModelDb.Power<FeiyapHeavenMayPower>().ToMutable(),
            Owner.Creature,
            1m,
            Owner.Creature,
            this);
    }
}
