using System.Collections.Generic;
using System.Threading.Tasks;
using Feiyap.Characters;
using Feiyap.Mechanics;
using Feiyap.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Scaffolding.Content;

namespace Feiyap.Cards.Basic;

/// <summary>
/// 体内灼烧：造成伤害并给予体内灼烧。
/// </summary>
[RegisterCard(typeof(FeiyapCardPool))]
[RegisterCharacterStarterCard(typeof(FeiyapCharacter), 1)]
public sealed class TiNeiZhuoShao : FeiyapCardTemplate
{
    private const int BaseEnergyCost = 1;
    private const CardType CardKind = CardType.Attack;
    private const CardRarity CardRarityValue = CardRarity.Basic;
    private const TargetType CardTarget = TargetType.AnyEnemy;
    private const bool ShowInCardLibrary = true;


    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        FeiyapKeywords.InternalBurn
    ];

    protected override HashSet<CardTag> CanonicalTags => new()
    {
        CardTag.Strike,
        FeiyapCardTags.InternalBurn
    };

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(6, ValueProp.Move),
        new PowerVar<FeiyapInternalBurnPower>(3m)
    ];

    public TiNeiZhuoShao() : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);

        await PowerCmd.Apply(
            choiceContext,
            ModelDb.Power<FeiyapInternalBurnPower>().ToMutable(),
            cardPlay.Target,
            DynamicVars["FeiyapInternalBurnPower"].BaseValue,
            Owner.Creature,
            this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3m);
    }
}
