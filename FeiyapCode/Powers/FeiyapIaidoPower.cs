using System.Collections.Generic;

using System.Linq;

using System.Threading.Tasks;

using Feiyap.Characters;

using Feiyap.Mechanics;

using Godot;

using MegaCrit.Sts2.Core.Commands;

using MegaCrit.Sts2.Core.Entities.Cards;

using MegaCrit.Sts2.Core.Entities.Creatures;

using MegaCrit.Sts2.Core.Entities.Powers;

using MegaCrit.Sts2.Core.Entities.Players;

using MegaCrit.Sts2.Core.GameActions.Multiplayer;

using MegaCrit.Sts2.Core.Models;

using MegaCrit.Sts2.Core.ValueProps;

using STS2RitsuLib.Combat.HealthBars;

using STS2RitsuLib.Interop.AutoRegistration;

using STS2RitsuLib.Scaffolding.Characters;

using STS2RitsuLib.Scaffolding.Content;



namespace Feiyap.Powers;



/// <summary>

/// 居合：受击时消耗等值居合并反击；打出攻击牌或回合开始时清空。

/// </summary>

[RegisterPower]

public sealed class FeiyapIaidoPower : ModPowerTemplate, IHealthBarForecastSource

{

    private static readonly Color IaidoForecastColor = new("6EBFD4");



    private int _pendingConsume;

    private decimal _pendingIncomingDamage;



    public override PowerType Type => PowerType.Buff;



    public override PowerStackType StackType => PowerStackType.Counter;



    protected override IEnumerable<string> RegisteredKeywordIds => [FeiyapKeywords.IaidoId];



    public IEnumerable<HealthBarForecastSegment> GetHealthBarForecastSegments(HealthBarForecastContext context)

    {

        if (context.Creature != Owner || Amount <= 0)

        {

            return [];

        }



        return HealthBarForecasts.FromRight(context, IaidoForecastColor)

            .Add(Amount)

            .Build();

    }



    public override decimal ModifyDamageAdditive(

        Creature? target,

        decimal amount,

        ValueProp props,

        Creature? dealer,

        CardModel? cardSource)

    {

        if (!ShouldConsumeIaido(target, dealer, amount))

        {

            ClearPendingConsume();

            return 0m;

        }



        var consume = (int)Math.Min(Amount, amount);

        if (consume <= 0)

        {

            ClearPendingConsume();

            return 0m;

        }



        _pendingConsume = consume;

        _pendingIncomingDamage = amount;

        return -consume;

    }



    public override async Task BeforeDamageReceived(

        PlayerChoiceContext choiceContext,

        Creature target,

        decimal amount,

        ValueProp props,

        Creature? dealer,

        CardModel? cardSource)

    {

        if (target != Owner || dealer == null || _pendingConsume <= 0 || dealer.Side == Owner.Side)

        {

            ClearPendingConsume();

            return;

        }



        var consume = _pendingConsume;

        var incomingDamage = _pendingIncomingDamage;

        ClearPendingConsume();



        Flash();



        await PowerCmd.Apply(choiceContext, this, Owner, -consume, Owner, null);



        if (consume == incomingDamage)

        {

            await FeiyapPerfectIaidoCmd.Trigger(choiceContext, Owner, consume);

        }



        var heavenMay = Owner.FindPower<FeiyapHeavenMayPower>();

        if (heavenMay != null)

        {

            var enemies = Owner.CombatState?.GetOpponentsOf(Owner).Where(e => e.IsAlive).ToList() ?? [];

            foreach (var enemy in enemies)

            {

                await CreatureCmd.Damage(

                    choiceContext,

                    enemy,

                    consume,

                    ValueProp.Unpowered | ValueProp.SkipHurtAnim,

                    Owner,

                    null);

            }

        }

        else

        {

            await CreatureCmd.Damage(

                choiceContext,

                dealer,

                consume,

                ValueProp.Unpowered | ValueProp.SkipHurtAnim,

                Owner,

                null);

        }

    }



    public override Task AfterModifyingDamageAmount(CardModel? cardSource)

    {

        if (_pendingConsume > 0)

        {

            Flash();

        }



        return Task.CompletedTask;

    }



    public override Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)

    {

        if (cardPlay.Card.Owner?.Creature != Owner || cardPlay.Card.Type != CardType.Attack)

        {

            return Task.CompletedTask;

        }



        return FeiyapIaidoCmd.ClearAll(choiceContext, Owner);

    }



    public override Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)

    {

        if (player.Creature != Owner)

        {

            return Task.CompletedTask;

        }



        return FeiyapIaidoCmd.ClearAll(choiceContext, Owner);

    }



    private bool ShouldConsumeIaido(Creature? target, Creature? dealer, decimal amount) =>

        target == Owner

        && dealer != null

        && amount > 0m

        && Amount > 0

        && dealer.Side != Owner.Side;



    private void ClearPendingConsume()

    {

        _pendingConsume = 0;

        _pendingIncomingDamage = 0m;

    }

}


