using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Feiyap.Powers;

/// <summary>
/// VII-战车：回合结束时从弃牌堆自动打出随机牌。
/// </summary>
[RegisterPower]
public sealed class FeiyapChariotPower : ModPowerTemplate
{
    private bool _isReversed;
    private bool _dualEffect;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => FeiyapPowerAssets.For(nameof(FeiyapChariotPower));

    public void SetReversed(bool reversed)
    {
        AssertMutable();
        _isReversed = reversed;
    }

    public void SetDualEffect(bool dual)
    {
        AssertMutable();
        _dualEffect = dual;
    }

    public override async Task BeforeSideTurnEndEarly(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (!participants.Contains(Owner) || Owner.Player == null || Amount <= 0)
        {
            return;
        }

        Flash();
        if (!_isReversed || _dualEffect)
        {
            await AutoPlayFromDiscard(choiceContext, CardType.Skill);
        }

        if (_isReversed || _dualEffect)
        {
            await AutoPlayFromDiscard(choiceContext, CardType.Attack);
        }
    }

    private async Task AutoPlayFromDiscard(PlayerChoiceContext choiceContext, CardType cardType)
    {
        var pile = PileType.Discard.GetPile(Owner.Player!);
        var candidates = pile.Cards
            .Where(c => c.Type == cardType && !c.Keywords.Contains(CardKeyword.Unplayable))
            .ToList()
            .StableShuffle(Owner.Player!.RunState.Rng.Shuffle)
            .Take((int)Amount)
            .ToList();

        if (candidates.Count == 0)
        {
            return;
        }

        foreach (var card in candidates)
        {
            if (CombatManager.Instance.IsOverOrEnding)
            {
                break;
            }

            if (card.TargetType == TargetType.AnyEnemy)
            {
                var target = Owner.Player!.RunState.Rng.CombatTargets
                    .NextItem(Owner.CombatState?.HittableEnemies ?? []);
                await CardCmd.AutoPlay(choiceContext, card, target);
            }
            else
            {
                await CardCmd.AutoPlay(choiceContext, card, null);
            }
        }
    }
}
