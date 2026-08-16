using Feiyap.Characters;
using Feiyap.Mechanics;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Scaffolding.Content;

namespace Feiyap.Cards.Rare;

/// <summary>
/// 幾星霜：获得居合；每次抽到时，仅增加本张牌本场战斗的居合获得量。
/// </summary>
[RegisterCard(typeof(FeiyapCardPool))]
public sealed class FeiyapRare10 : FeiyapCardTemplate
{
    private decimal _combatDrawBonus;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [FeiyapKeywords.Iaido];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new StarFrostIaidoVar(this, 4m, ValueProp.Move),
        new DynamicVar("DrawBonus", 4m)
    ];

    public FeiyapRare10()
        : base(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }

    public override Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
    {
        if (card != this)
        {
            return Task.CompletedTask;
        }

        AssertMutable();
        _combatDrawBonus += DynamicVars["DrawBonus"].BaseValue;
        return Task.CompletedTask;
    }

    public override Task BeforeCombatStart()
    {
        _combatDrawBonus = 0m;
        return Task.CompletedTask;
    }

    public override Task AfterCombatEnd(CombatRoom room)
    {
        _combatDrawBonus = 0m;
        return Task.CompletedTask;
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await FeiyapIaidoCmd.Gain(
            choiceContext,
            Owner.Creature,
            DynamicVars[IaidoVar.DefaultName].BaseValue + _combatDrawBonus,
            ValueProp.Move,
            this,
            cardPlay);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["DrawBonus"].UpgradeValueBy(2m);
    }

    /// <summary>预览时把本场抽牌累计加成算进居合基础值。</summary>
    private sealed class StarFrostIaidoVar : DynamicVar
    {
        private readonly FeiyapRare10 _card;

        public ValueProp Props { get; }

        public StarFrostIaidoVar(FeiyapRare10 card, decimal iaido, ValueProp props)
            : base(IaidoVar.DefaultName, iaido)
        {
            _card = card;
            Props = props;
        }

        public override void UpdateCardPreview(
            CardModel card,
            CardPreviewMode previewMode,
            Creature? target,
            bool runGlobalHooks)
        {
            var amount = BaseValue + _card._combatDrawBonus;

            if (runGlobalHooks && card.Owner?.Creature is { } creature)
            {
                amount = FeiyapIaidoCmd.PreviewGain(
                    creature,
                    BaseValue + _card._combatDrawBonus,
                    Props,
                    card);
            }

            PreviewValue = amount;
        }
    }
}
