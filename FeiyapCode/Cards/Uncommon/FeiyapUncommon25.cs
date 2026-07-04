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
/// IX-隐者：正位返还下次消耗的活力，逆位返还下次消耗的残心。
/// </summary>
[RegisterCard(typeof(FeiyapCardPool))]
public sealed class FeiyapUncommon25 : FeiyapTarotCardBase
{
    public FeiyapUncommon25()
        : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
        RegisterTarotFactory(player => player.RunState.CreateCard<FeiyapUncommon25>(player));
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        EnsureOrientationInitialized();

        var power = (FeiyapHermitPower)ModelDb.Power<FeiyapHermitPower>().ToMutable();
        if (FeiyapTarotCmd.HasDualEffect(Owner))
        {
            power.SetDualEffect(true);
        }
        else
        {
            power.SetReversed(await FeiyapTarotCmd.ResolveEffectiveReversed(choiceContext, this));
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
