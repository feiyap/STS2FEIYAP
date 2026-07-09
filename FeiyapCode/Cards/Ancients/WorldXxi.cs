using System.Threading.Tasks;
using Feiyap.Mechanics;
using Feiyap.Characters;
using Feiyap.Cards.Tarot;
using Feiyap.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Characters;
using STS2RitsuLib.Scaffolding.Content;

namespace Feiyap.Cards.Ancients;

/// <summary>
/// 先古卡：XXI-世界。
/// </summary>
[RegisterCard(typeof(FeiyapCardPool))]
public sealed class WorldXxi : FeiyapTarotCardBase
{
    public WorldXxi()
        : base(2, CardType.Power, CardRarity.Ancient, TargetType.Self, showInCardLibrary: true)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        EnsureOrientationInitialized();
        await CreatureCmd.TriggerAnim(Owner.Creature, "PowerUp", Owner.Character.PowerUpAnimDelay);

        if (FeiyapTarotCmd.HasDualEffect(Owner))
        {
            await ApplyWorldPowerOnce<FeiyapTarotWorldFreeChoicePower>(choiceContext);
            await ApplyWorldPowerOnce<FeiyapTarotWorldDualEffectPower>(choiceContext);
            return;
        }

        await RunTarotBranches(
            choiceContext,
            () => ApplyWorldPowerOnce<FeiyapTarotWorldFreeChoicePower>(choiceContext),
            () => ApplyWorldPowerOnce<FeiyapTarotWorldDualEffectPower>(choiceContext));
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }

    private async Task ApplyWorldPowerOnce<TPower>(PlayerChoiceContext choiceContext)
        where TPower : PowerModel
    {
        if (Owner.Creature.FindPower<TPower>() != null)
        {
            return;
        }

        await PowerCmd.Apply(
            choiceContext,
            ModelDb.Power<TPower>().ToMutable(),
            Owner.Creature,
            1m,
            Owner.Creature,
            this);
    }
}
