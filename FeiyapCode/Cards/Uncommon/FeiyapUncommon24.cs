using Feiyap.Mechanics;

using Feiyap.Characters;

using Feiyap.Cards.Tarot;

using Feiyap.Powers;

using MegaCrit.Sts2.Core.Commands;

using MegaCrit.Sts2.Core.Entities.Creatures;

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



        if (FeiyapTarotCmd.HasDualEffect(Owner))

        {

            await ApplyLoversUprightPower(choiceContext, cardPlay.Target);

            await ApplyLoversReversedPower(choiceContext, cardPlay.Target);

            return;

        }



        var reversed = await FeiyapTarotCmd.ResolveEffectiveReversed(choiceContext, this);

        if (reversed)

        {

            await ApplyLoversReversedPower(choiceContext, cardPlay.Target);

        }

        else

        {

            await ApplyLoversUprightPower(choiceContext, cardPlay.Target);

        }

    }



    private async Task ApplyLoversUprightPower(PlayerChoiceContext choiceContext, Creature target)

    {

        var power = (FeiyapLoversUprightPower)ModelDb.Power<FeiyapLoversUprightPower>().ToMutable();
        power.SetTarget(target);

        await PowerCmd.Apply(

            choiceContext,

            power,

            Owner.Creature,

            1m,

            Owner.Creature,

            this);

    }



    private async Task ApplyLoversReversedPower(PlayerChoiceContext choiceContext, Creature target)

    {

        var power = ModelDb.Power<FeiyapLoversReversedPower>().ToMutable();

        await PowerCmd.Apply(

            choiceContext,

            power,

            target,

            1m,

            Owner.Creature,

            this);

    }



    protected override void OnUpgrade()

    {

        EnergyCost.UpgradeBy(-1);

    }

}

