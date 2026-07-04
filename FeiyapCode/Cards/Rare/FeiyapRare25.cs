using Feiyap.Mechanics;
using Feiyap.Characters;
using Feiyap.Cards.Tarot;
using Feiyap.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Feiyap.Cards.Rare;

/// <summary>
/// VII-战车：正位回合结束打出弃牌堆技能，逆位打出弃牌堆攻击。
/// </summary>
[RegisterCard(typeof(FeiyapCardPool))]
public sealed class FeiyapRare25 : FeiyapTarotCardBase
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<FeiyapChariotPower>(1m)
    ];

    public FeiyapRare25()
        : base(1, CardType.Power, CardRarity.Rare, TargetType.Self)
    {
        RegisterTarotFactory(player => player.RunState.CreateCard<FeiyapRare25>(player));
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        EnsureOrientationInitialized();
        await CreatureCmd.TriggerAnim(Owner.Creature, "PowerUp", Owner.Character.PowerUpAnimDelay);

        var power = (FeiyapChariotPower)ModelDb.Power<FeiyapChariotPower>().ToMutable();
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
            DynamicVars["FeiyapChariotPower"].BaseValue,
            Owner.Creature,
            this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["FeiyapChariotPower"].UpgradeValueBy(1m);
    }
}
