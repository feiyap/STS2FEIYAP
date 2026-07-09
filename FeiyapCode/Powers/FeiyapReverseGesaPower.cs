using System.Threading.Tasks;
using Feiyap.Mechanics;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Feiyap.Powers;

/// <summary>
/// 逆袈裟：下一张牌获得的格挡与居合翻倍。
/// </summary>
[RegisterPower]
public sealed class FeiyapReverseGesaPower : ModPowerTemplate, IFeiyapIaidoGainMultiplier
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override PowerAssetProfile AssetProfile => FeiyapPowerAssets.For(nameof(FeiyapReverseGesaPower));

    public decimal ModifyIaidoGainMultiplicative(in FeiyapIaidoGainContext context, decimal amount)
    {
        if (context.Creature != Owner || amount <= 0m)
        {
            return amount;
        }

        return amount * 2m;
    }

    public override decimal ModifyBlockMultiplicative(
        Creature target,
        decimal block,
        ValueProp props,
        CardModel? cardSource,
        CardPlay? cardPlay)
    {
        if (target != Owner || block <= 0m || !props.HasFlag(ValueProp.Move))
        {
            return 1m;
        }

        return 2m;
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner?.Creature != Owner)
        {
            return;
        }

        await PowerCmd.Remove(this);
    }
}
