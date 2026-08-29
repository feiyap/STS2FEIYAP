using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Feiyap.Cards.Ancients;
using Feiyap.Characters;
using Feiyap.Mechanics;
using Feiyap.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Feiyap.Relics;

public abstract class InvestigatorBase : ModRelicTemplate
{
    protected abstract int EnemyCount { get; }

    protected abstract int PozhanAmount { get; }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<FeiyapPozhanPower>(PozhanAmount)
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromCard(ModelDb.Card<KeXueZheng>()),
        HoverTipFactory.FromPower<FeiyapPozhanPower>()
    ];

    public override async Task AfterObtained()
    {
        if (FeiyapQuestRewards.SuppressQuestRelicObtainEffects)
        {
            return;
        }

        await FeiyapQuestRewards.GainAncientCard<KeXueZheng>(Owner, this is FeiShengYiWenZi);
    }

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner)
        {
            return;
        }

        var enemies = Owner.Creature.CombatState?.HittableEnemies;
        if (enemies == null)
        {
            return;
        }

        var targets = enemies.TakeRandom(EnemyCount, Owner.RunState.Rng.CombatTargets).ToList();
        if (targets.Count == 0)
        {
            return;
        }

        Flash();
        foreach (var target in targets)
        {
            await PowerCmd.Apply<FeiyapPozhanPower>(
                choiceContext,
                target,
                PozhanAmount,
                Owner.Creature,
                null);
        }
    }
}

[RegisterRelic(typeof(FeiyapRelicPool))]
public sealed class Investigator : InvestigatorBase
{
    protected override int EnemyCount => 1;

    protected override int PozhanAmount => 1;

    public override RelicRarity Rarity => RelicRarity.Event;

    public override RelicAssetProfile AssetProfile => FeiyapRelicAssets.For(nameof(Investigator));
}

[RegisterRelic(typeof(FeiyapRelicPool))]
public sealed class FeiShengYiWenZi : InvestigatorBase
{
    protected override int EnemyCount => 2;

    protected override int PozhanAmount => 2;

    public override RelicRarity Rarity => RelicRarity.Event;

    public override RelicAssetProfile AssetProfile => FeiyapRelicAssets.For(nameof(FeiShengYiWenZi));
}
