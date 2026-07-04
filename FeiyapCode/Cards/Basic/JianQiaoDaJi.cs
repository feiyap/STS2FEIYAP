using Feiyap.Characters;
using Feiyap.Mechanics;
using Feiyap.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Feiyap.Cards.Basic;

/// <summary>
/// 剑鞘打击：造成 3 / 6 点伤害，附加等量居合值；本次攻击不消耗居合。
/// </summary>
[RegisterCard(typeof(FeiyapCardPool))]
[RegisterCharacterStarterCard(typeof(FeiyapCharacter), 1)]
public sealed class JianQiaoDaJi : FeiyapCardTemplate
{
    protected override HashSet<CardTag> CanonicalTags => new()
    {
        CardTag.Strike,
        FeiyapCardTags.SkipIaidoConsume
    };

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(3, ValueProp.Move)
    ];

    public JianQiaoDaJi()
        : base(0, CardType.Attack, CardRarity.Basic, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);

        var iaidoBonus = Owner.Creature.GetPowerAmount<FeiyapIaidoPower>();
        var damage = DynamicVars.Damage.BaseValue + iaidoBonus;

        await DamageCmd.Attack(damage)
            .FromCard(this)
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);

        if (IsUpgraded)
        {
            await PowerCmd.Apply<WeakPower>(
                choiceContext,
                cardPlay.Target,
                1m,
                Owner.Creature,
                this);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3m);
    }
}
