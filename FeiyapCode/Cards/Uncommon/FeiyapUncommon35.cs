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
/// 0-愚者：正位回合开始敌人失去力量，逆位抽技能牌并赋予虚无与减费。
/// </summary>
[RegisterCard(typeof(FeiyapCardPool))]
public sealed class FeiyapUncommon35 : FeiyapTarotCardBase
{
    public FeiyapUncommon35()
        : base(2, CardType.Power, CardRarity.Uncommon, TargetType.Self)
    {
        RegisterTarotFactory(player => player.RunState.CreateCard<FeiyapUncommon35>(player));
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        EnsureOrientationInitialized();
        await CreatureCmd.TriggerAnim(Owner.Creature, "PowerUp", Owner.Character.PowerUpAnimDelay);

        if (FeiyapTarotCmd.HasDualEffect(Owner))
        {
            await ApplyFoolPower<FeiyapFoolUprightPower>(choiceContext);
            await ApplyFoolPower<FeiyapFoolReversedPower>(choiceContext);
            return;
        }

        var reversed = await FeiyapTarotCmd.ResolveEffectiveReversed(choiceContext, this);
        if (reversed)
        {
            await ApplyFoolPower<FeiyapFoolReversedPower>(choiceContext);
        }
        else
        {
            await ApplyFoolPower<FeiyapFoolUprightPower>(choiceContext);
        }
    }

    private async Task ApplyFoolPower<TPower>(PlayerChoiceContext choiceContext)
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
