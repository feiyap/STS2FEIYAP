using System.Linq;
using Feiyap.Characters;
using Feiyap.Mechanics;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Scaffolding.Content;

namespace Feiyap.Cards.Rare;

/// <summary>
/// 拦截（多人专属）：所有队友（含自己）获得等于你当前居合的居合。
/// </summary>
[RegisterCard(typeof(FeiyapCardPool))]
public sealed class FeiyapRare13 : FeiyapCardTemplate
{
    public override CardMultiplayerConstraint MultiplayerConstraint =>
        CardMultiplayerConstraint.MultiplayerOnly;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [FeiyapKeywords.Iaido];

    public FeiyapRare13()
        : base(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var amount = FeiyapIaidoCmd.GetNumericAmount(Owner.Creature);
        if (amount <= 0)
        {
            return;
        }

        if (CombatState is not CombatState state)
        {
            return;
        }

        foreach (var ally in GetAllAllies(state))
        {
            await FeiyapIaidoCmd.Gain(
                choiceContext,
                ally,
                amount,
                ValueProp.Move,
                this,
                cardPlay);
        }
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }

    private IEnumerable<Creature> GetAllAllies(CombatState state) =>
        state.PlayerCreatures.Where(c => c.IsAlive && c.IsPlayer);
}
