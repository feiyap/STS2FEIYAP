using Feiyap.Characters;
using Feiyap.Cards.Tarot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Feiyap.Cards.Rare;

/// <summary>
/// XIII-死神：X 费攻击；可无限升级，每次随机提升伤害、段数或重放次数之一。
/// </summary>
[RegisterCard(typeof(FeiyapCardPool))]
public sealed class FeiyapRare7 : FeiyapTarotCardBase
{
    private int _dmgBonus;
    private int _hitBonus;
    private int _replayBonus;

    protected override bool HasEnergyCostX => true;

    public override int MaxUpgradeLevel => 999;

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromPower<WeakPower>(),
        HoverTipFactory.FromPower<VulnerablePower>()
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("DmgBonus", 0m),
        new DynamicVar("HitBonus", 0m),
        new DynamicVar("ReplayBonus", 0m)
    ];

    [SavedProperty]
    public int DmgBonus
    {
        get => _dmgBonus;
        set
        {
            AssertMutable();
            _dmgBonus = Math.Max(0, value);
            DynamicVars["DmgBonus"].BaseValue = _dmgBonus;
        }
    }

    [SavedProperty]
    public int HitBonus
    {
        get => _hitBonus;
        set
        {
            AssertMutable();
            _hitBonus = Math.Max(0, value);
            DynamicVars["HitBonus"].BaseValue = _hitBonus;
        }
    }

    [SavedProperty]
    public int ReplayBonus
    {
        get => _replayBonus;
        set
        {
            AssertMutable();
            _replayBonus = Math.Max(0, value);
            DynamicVars["ReplayBonus"].BaseValue = _replayBonus;
        }
    }

    public FeiyapRare7()
        : base(0, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
    {
        RegisterTarotFactory(player => player.RunState.CreateCard<FeiyapRare7>(player));
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        EnsureOrientationInitialized();
        ArgumentNullException.ThrowIfNull(cardPlay.Target);

        var x = ResolveEnergyXValue();
        var damage = x + DmgBonus;
        var hits = x + HitBonus;
        var replays = x + ReplayBonus;

        for (var replay = 0; replay < replays; replay++)
        {
            await DamageCmd.Attack(damage)
                .WithHitCount(hits)
                .FromCard(this, cardPlay)
                .Targeting(cardPlay.Target)
                .Execute(choiceContext);
        }

        await RunTarotBranches(
            choiceContext,
            async () =>
            {
                await PowerCmd.Apply<WeakPower>(
                    choiceContext,
                    cardPlay.Target,
                    x,
                    Owner.Creature,
                    this);
            },
            async () =>
            {
                await PowerCmd.Apply<VulnerablePower>(
                    choiceContext,
                    cardPlay.Target,
                    x,
                    Owner.Creature,
                    this);
            });
    }

    protected override void OnUpgrade()
    {
        var rng = Owner?.RunState.Rng.Niche ?? new Rng(0);
        switch (rng.NextInt(3))
        {
            case 0:
                DmgBonus++;
                break;
            case 1:
                HitBonus++;
                break;
            default:
                ReplayBonus++;
                break;
        }
    }
}
