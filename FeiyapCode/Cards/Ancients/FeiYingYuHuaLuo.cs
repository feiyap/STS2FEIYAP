using System.Collections.Generic;
using System.Threading.Tasks;
using Feiyap.Characters;
using Feiyap.Mechanics;
using Feiyap.Patches;
using Feiyap.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Scaffolding.Content;

namespace Feiyap.Cards.Ancients;

/// <summary>
/// 先古卡：绯樱狱华落。
/// </summary>
[RegisterCard(typeof(FeiyapCardPool))]
public sealed class FeiYingYuHuaLuo : FeiyapCardTemplate
{
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Exhaust,
        FeiyapKeywords.Iaido
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromPower<FeiyapInfiniteIaidoPower>()
    ];

    public FeiYingYuHuaLuo()
        : base(3, CardType.Skill, CardRarity.Ancient, TargetType.Self, showInCardLibrary: true)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await FeiyapIaidoCmd.ClearAll(choiceContext, Owner.Creature);

        await PowerCmd.Apply(
            choiceContext,
            ModelDb.Power<FeiyapInfiniteIaidoPower>().ToMutable(),
            Owner.Creature,
            1m,
            Owner.Creature,
            this);

        if (IsUpgraded)
        {
            await PowerCmd.Apply(
                choiceContext,
                ModelDb.Power<FeiyapIaidoSurgePower>().ToMutable(),
                Owner.Creature,
                1m,
                Owner.Creature,
                this);
        }

        IaidoHealthBarOverlay.RefreshForCreature(Owner.Creature);
    }

    protected override void OnUpgrade()
    {
        // 强化后本回合所有居合视为完美居合。
    }
}
