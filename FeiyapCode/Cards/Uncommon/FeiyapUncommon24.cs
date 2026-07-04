using Feiyap.Mechanics;
using Feiyap.Characters;
using Feiyap.Cards.Tarot;
using Feiyap.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Feiyap.Cards.Uncommon;

/// <summary>
/// VI-恋人：选择敌人；正位反弹自身受伤，逆位反弹目标受伤。
/// </summary>
[RegisterCard(typeof(FeiyapCardPool))]
public sealed class FeiyapUncommon24 : FeiyapTarotCardBase
{
    public FeiyapUncommon24()
        : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
        RegisterTarotFactory(player => player.RunState.CreateCard<FeiyapUncommon24>(player));
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        EnsureOrientationInitialized();
        ArgumentNullException.ThrowIfNull(cardPlay.Target);

        var power = (FeiyapLoversPower)ModelDb.Power<FeiyapLoversPower>().ToMutable();
        if (FeiyapTarotCmd.HasDualEffect(Owner))
        {
            power.Configure(cardPlay.Target, reversed: false);
            power.SetDualEffect(true);
        }
        else
        {
            power.Configure(
                cardPlay.Target,
                await FeiyapTarotCmd.ResolveEffectiveReversed(choiceContext, this));
        }
        await PowerCmd.Apply(
            choiceContext,
            power,
            Owner.Creature,
            1m,
            Owner.Creature,
            this);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
