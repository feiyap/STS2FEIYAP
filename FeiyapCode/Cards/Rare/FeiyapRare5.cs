using Feiyap.Characters;
using Feiyap.Mechanics;
using Feiyap.Powers;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Scaffolding.Content;

namespace Feiyap.Cards.Rare;

/// <summary>
/// 霜雪雷电闪：条件秒杀；握牌结束回合获得居合。消耗。
/// </summary>
[RegisterCard(typeof(FeiyapCardPool))]
public sealed class FeiyapRare5 : FeiyapCardTemplate
{
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Retain,
        CardKeyword.Exhaust,
        FeiyapKeywords.Iaido
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new IaidoVar(7m, ValueProp.Move),
        new DynamicVar("ExecuteThreshold", 25m)
    ];

    public override bool HasTurnEndInHandEffect => true;

    public FeiyapRare5()
        : base(3, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);

        if (ShouldExecute(cardPlay.Target))
        {
            await CreatureCmd.Kill(cardPlay.Target);
            return;
        }
    }

    protected override async Task OnTurnEndInHand(PlayerChoiceContext choiceContext)
    {
        await FeiyapIaidoCmd.Gain(
            choiceContext,
            Owner.Creature,
            DynamicVars[IaidoVar.DefaultName].BaseValue,
            ValueProp.Move,
            this,
            null);
    }

    protected override void OnUpgrade()
    {
        DynamicVars[IaidoVar.DefaultName].UpgradeValueBy(4m);
        DynamicVars["ExecuteThreshold"].UpgradeValueBy(15m);
    }

    private bool ShouldExecute(Creature target)
    {
        if (!target.IsAlive)
        {
            return false;
        }

        var roomType = CombatState?.Encounter?.RoomType;
        if (roomType is not (RoomType.Elite or RoomType.Boss))
        {
            return true;
        }

        if (target.MaxHp <= 0m)
        {
            return false;
        }

        return target.CurrentHp / target.MaxHp * 100m < DynamicVars["ExecuteThreshold"].BaseValue;
    }
}
