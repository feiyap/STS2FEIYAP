using Feiyap.Characters;
using Feiyap.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Feiyap.Cards.Rare;

/// <summary>
/// 枯山水：移除所有敌人的防御增益并禁止再获得，然后对所有敌人造成伤害。
/// </summary>
[RegisterCard(typeof(FeiyapCardPool))]
public sealed class FeiyapRare4 : FeiyapCardTemplate
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromPower<FeiyapKaresansuiPower>(),
        HoverTipFactory.FromPower<SlipperyPower>(),
        HoverTipFactory.FromPower<BufferPower>(),
        HoverTipFactory.FromPower<HardToKillPower>(),
        HoverTipFactory.FromPower<HardenedShellPower>()
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(10, ValueProp.Move)
    ];

    public FeiyapRare4()
        : base(1, CardType.Attack, CardRarity.Rare, TargetType.AllEnemies)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (CombatState == null)
        {
            return;
        }

        foreach (var enemy in CombatState.HittableEnemies)
        {
            if (enemy.Block > 0m)
            {
                await CreatureCmd.LoseBlock(
                    choiceContext,
                    enemy,
                    enemy.Block,
                    Owner.Creature);
            }

            await FeiyapKaresansuiPower.StripForbiddenPowers(enemy);
            await PowerCmd.Apply(
                choiceContext,
                ModelDb.Power<FeiyapKaresansuiPower>().ToMutable(),
                enemy,
                1m,
                Owner.Creature,
                this);
        }

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, cardPlay)
            .TargetingAllOpponents(CombatState)
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(5m);
    }
}
