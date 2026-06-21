using System.Collections.Generic;
using System.Threading.Tasks;
using Feiyap.Cards.Ancients;
using Feiyap.Characters;
using Feiyap.Mechanics;
using Feiyap.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Feiyap.Relics;

public abstract class StarSlayerBase : ModRelicTemplate
{
    protected abstract decimal VoidHoleAmount { get; }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<FeiyapVoidHolePower>(VoidHoleAmount)
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromCard(ModelDb.Card<WuShengXingYun>()),
        HoverTipFactory.FromPower<FeiyapVoidHolePower>()
    ];

    protected override IEnumerable<string> RegisteredKeywordIds => [FeiyapKeywords.VoidHoleId];

    public override async Task AfterObtained()
    {
        await FeiyapQuestRewards.GainAncientCard<WuShengXingYun>(Owner);
    }

    public override async Task BeforeCombatStart()
    {
        var combatState = Owner.Creature.CombatState;
        if (combatState is not CombatState state)
        {
            return;
        }

        Flash();
        var context = new ThrowingPlayerChoiceContext();
        foreach (var enemy in state.HittableEnemies)
        {
            await PowerCmd.Apply(
                context,
                ModelDb.Power<FeiyapVoidHolePower>().ToMutable(),
                enemy,
                VoidHoleAmount,
                Owner.Creature,
                null);
        }
    }
}

[RegisterRelic(typeof(FeiyapRelicPool))]
public sealed class StarSlayer : StarSlayerBase
{
    protected override decimal VoidHoleAmount => 1m;

    public override RelicRarity Rarity => RelicRarity.Rare;

    public override RelicAssetProfile AssetProfile => FeiyapRelicAssets.For(nameof(StarSlayer));
}

[RegisterRelic(typeof(FeiyapRelicPool))]
public sealed class TianXingJian : StarSlayerBase
{
    protected override decimal VoidHoleAmount => 2m;

    public override RelicRarity Rarity => RelicRarity.Rare;

    public override RelicAssetProfile AssetProfile => FeiyapRelicAssets.For(nameof(TianXingJian));
}
