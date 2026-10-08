using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Feiyap.Cards.Ancients;
using Feiyap.Characters;
using Feiyap.Mechanics;
using Feiyap.Powers;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Saves.Runs;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Feiyap.Relics;

public abstract class SwordSaintBase : ModRelicTemplate
{
    private bool _isActivating;
    private int _turnsSeen;

    /// <summary>每隔多少个回合获得 1 层居合强化。</summary>
    protected abstract int TurnInterval { get; }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<FeiyapIaidoEnhancePower>(1m),
        new DynamicVar("Turns", TurnInterval)
    ];

    protected override IEnumerable<string> RegisteredKeywordIds => [FeiyapKeywords.IaidoEnhanceId];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromCard(ModelDb.Card<FeiYingYuHuaLuo>()),
        HoverTipFactory.FromPower<FeiyapIaidoEnhancePower>()
    ];

    public override bool ShowCounter => TurnInterval > 1;

    public override int DisplayAmount =>
        IsActivating ? DynamicVars["Turns"].IntValue : TurnsSeen;

    private bool IsActivating
    {
        get => _isActivating;
        set
        {
            AssertMutable();
            _isActivating = value;
            InvokeDisplayAmountChanged();
        }
    }

    /// <summary>本局已累计的回合数，满间隔后归零并触发。跨战斗保留。</summary>
    [SavedProperty]
    public int TurnsSeen
    {
        get => _turnsSeen;
        set
        {
            AssertMutable();
            _turnsSeen = value;
            InvokeDisplayAmountChanged();
        }
    }

    public override async Task AfterObtained()
    {
        if (FeiyapQuestRewards.SuppressQuestRelicObtainEffects)
        {
            return;
        }

        await FeiyapQuestRewards.GainAncientCard<FeiYingYuHuaLuo>(Owner, this is WuMingRen);
    }

    public override async Task AfterSideTurnStart(
        CombatSide side,
        IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        if (!participants.Contains(Owner.Creature))
        {
            return;
        }

        var interval = DynamicVars["Turns"].IntValue;
        TurnsSeen = (TurnsSeen + 1) % interval;
        Status = interval > 1 && TurnsSeen == interval - 1
            ? RelicStatus.Active
            : RelicStatus.Normal;

        if (TurnsSeen != 0)
        {
            return;
        }

        _ = TaskHelper.RunSafely(DoActivateVisuals());
        await PowerCmd.Apply<FeiyapIaidoEnhancePower>(
            new ThrowingPlayerChoiceContext(),
            Owner.Creature,
            DynamicVars["FeiyapIaidoEnhancePower"].BaseValue,
            Owner.Creature,
            null);
    }

    public override Task AfterCombatEnd(CombatRoom _)
    {
        Status = RelicStatus.Normal;
        return Task.CompletedTask;
    }

    private async Task DoActivateVisuals()
    {
        IsActivating = true;
        Flash();
        await Cmd.Wait(1f);
        IsActivating = false;
    }
}

[RegisterRelic(typeof(FeiyapRelicPool))]
public sealed class SwordSaint : SwordSaintBase
{
    protected override int TurnInterval => 2;

    public override RelicRarity Rarity => RelicRarity.Event;

    public override RelicAssetProfile AssetProfile => FeiyapRelicAssets.For(nameof(SwordSaint));
}

[RegisterRelic(typeof(FeiyapRelicPool))]
public sealed class WuMingRen : SwordSaintBase
{
    protected override int TurnInterval => 1;

    public override RelicRarity Rarity => RelicRarity.Event;

    public override RelicAssetProfile AssetProfile => FeiyapRelicAssets.For(nameof(WuMingRen));
}
