using System.Linq;
using Feiyap.Characters;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Feiyap.Cards.Rare;

/// <summary>
/// XVIII-月亮：所有玩家获得格挡；握牌时打出技能牌后追加格挡并变为 XIX-太阳。
/// </summary>
[RegisterCard(typeof(FeiyapCardPool))]
public sealed class FeiyapRare12 : FeiyapCardTemplate
{
    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(10, ValueProp.Move),
        new DynamicVar("TriggerBlock", 5m)
    ];

    public FeiyapRare12()
        : base(1, CardType.Skill, CardRarity.Rare, TargetType.AllAllies)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await GainBlockForAllPlayers(choiceContext, DynamicVars.Block, cardPlay);
    }

    public override async Task AfterCardPlayedLate(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner != Owner
            || cardPlay.Card.Type != CardType.Skill
            || cardPlay.Card == this
            || Pile?.Type != PileType.Hand
            || CombatState == null)
        {
            return;
        }

        await GainBlockForAllPlayers(
            choiceContext,
            DynamicVars["TriggerBlock"].BaseValue,
            cardPlay: null);

        var shouldUpgrade = IsUpgraded;
        TaskHelper.RunSafely(TransformToSunAsync(shouldUpgrade));
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(5m);
        DynamicVars["TriggerBlock"].UpgradeValueBy(3m);
    }

    private async Task GainBlockForAllPlayers(
        PlayerChoiceContext choiceContext,
        BlockVar blockVar,
        CardPlay? cardPlay)
    {
        if (CombatState == null)
        {
            return;
        }

        foreach (var player in GetLivingPlayerCreatures())
        {
            await CreatureCmd.GainBlock(player, blockVar, cardPlay);
        }
    }

    private async Task GainBlockForAllPlayers(
        PlayerChoiceContext choiceContext,
        decimal amount,
        CardPlay? cardPlay)
    {
        if (CombatState == null)
        {
            return;
        }

        foreach (var player in GetLivingPlayerCreatures())
        {
            await CreatureCmd.GainBlock(player, amount, ValueProp.Move, cardPlay);
        }
    }

    private IEnumerable<Creature> GetLivingPlayerCreatures() =>
        CombatState!.PlayerCreatures.Where(c => c.IsAlive && c.IsPlayer);

    private async Task TransformToSunAsync(bool shouldUpgrade)
    {
        if (Pile?.Type != PileType.Hand || CombatState == null)
        {
            return;
        }

        var sun = CombatState.CreateCard<FeiyapRare6>(Owner);
        if (shouldUpgrade)
        {
            CardCmd.Upgrade(sun);
        }

        await CardCmd.Transform(this, sun, CardPreviewStyle.None);
    }
}
