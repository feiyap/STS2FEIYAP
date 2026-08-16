using System.Globalization;
using System.Linq;
using Feiyap.Characters;
using Feiyap.Mechanics;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Scaffolding.Content;

namespace Feiyap.Cards.Rare;

/// <summary>
/// 流风遗韵：依据本回合造成的伤害量获得等量居合。
/// </summary>
[RegisterCard(typeof(FeiyapCardPool))]
public sealed class FeiyapRare9 : FeiyapCardTemplate
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [FeiyapKeywords.Iaido];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new RecordedDamageVar()
    ];

    public FeiyapRare9()
        : base(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var player = Owner;
        if (player == null)
        {
            return;
        }

        var amount = GetTurnDamageDealt(this);
        if (amount <= 0)
        {
            return;
        }

        await FeiyapIaidoCmd.Gain(
            choiceContext,
            player.Creature,
            amount,
            ValueProp.Move,
            this,
            cardPlay);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }

    /// <summary>
    /// 从战斗历史统计本回合已造成伤害（含格挡吸收与溢出），与原版「本回合」查询一致。
    /// </summary>
    private static int GetTurnDamageDealt(CardModel card)
    {
        if (card.Owner?.Creature is not { } dealer
            || card.CombatState == null
            || !CombatManager.Instance.IsInProgress)
        {
            return 0;
        }

        return CombatManager.Instance.History.Entries
            .OfType<DamageReceivedEntry>()
            .Where(entry =>
                entry.HappenedThisTurn(card.CombatState)
                && entry.Dealer != null
                && (entry.Dealer == dealer || entry.Dealer.PetOwner?.Creature == dealer))
            .Sum(entry => entry.Result.TotalDamage + entry.Result.OverkillDamage);
    }

    /// <summary>同步本回合已造成伤害，供卡牌描述预览与打出结算共用。</summary>
    private sealed class RecordedDamageVar : DynamicVar
    {
        public RecordedDamageVar()
            : base("RecordedDamage", 0m)
        {
        }

        public override void UpdateCardPreview(
            CardModel card,
            CardPreviewMode previewMode,
            Creature? target,
            bool runGlobalHooks)
        {
            PreviewValue = GetTurnDamageDealt(card);
        }

        protected override decimal GetBaseValueForIConvertible()
        {
            if (_owner is not CardModel card)
            {
                return BaseValue;
            }

            return GetTurnDamageDealt(card);
        }

        public override string ToString() =>
            ((int)GetBaseValueForIConvertible()).ToString(CultureInfo.InvariantCulture);
    }
}
