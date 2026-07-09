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

        if (FeiyapTarotCmd.HasDualEffect(Owner))
        {
            await ApplyHermitPower<FeiyapHermitUprightPower>(choiceContext);
            await ApplyHermitPower<FeiyapHermitReversedPower>(choiceContext);
            return;
        }

        var reversed = await FeiyapTarotCmd.ResolveEffectiveReversed(choiceContext, this);
        if (reversed)
        {
            await ApplyHermitPower<FeiyapHermitReversedPower>(choiceContext);
        }
        else
        {
            await ApplyHermitPower<FeiyapHermitUprightPower>(choiceContext);
        }
    }

    private async Task ApplyHermitPower<TPower>(PlayerChoiceContext choiceContext)
        where TPower : ModPowerTemplate
    {
        var power = ModelDb.Power<TPower>().ToMutable();
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
