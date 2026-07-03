using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Danjin.Extensions;
using Danjin.Patches;
using Danjin.Powers;
using Danjin.Resources;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Danjin.Cards;

public class JianWu : DanjinCard
{
	protected override HashSet<CardTag> CanonicalTags => new HashSet<CardTag> { (CardTag)0 };

	protected override IEnumerable<DynamicVar> CanonicalVars => new _003C_003Ez__ReadOnlyArray<DynamicVar>((DynamicVar[])(object)new DynamicVar[3]
	{
		new DynamicVar("HpLossPercent", 4m),
		(DynamicVar)new EnergyVar(2),
		(DynamicVar)new PowerVar<FengMangPower>(1m)
	});

	protected override IEnumerable<IHoverTip> AdditionalHoverTips
	{
		get
		{
			yield return BuildHpLossTip();
			yield return HoverTipFactory.FromPower<FengMangPower>((int?)null);
			yield return RawHoverTipBuilder.Build("danjin-stance-atk", "攻势1", "打出时需要至少[blue]1[/blue]层[red]攻势[/red]，否则该效果不生效。");
		}
	}

	public JianWu()
		: base(0, (CardType)2, (CardRarity)2, (TargetType)1)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		Creature creature = ((CardModel)this).Owner.Creature;
		int gongShi = ((creature != null) ? creature.GetPowerAmount<GongShiPower>() : 0);
		int num = (int)((CardModel)this).DynamicVars["HpLossPercent"].BaseValue;
		decimal num2 = Math.Floor((decimal)creature.MaxHp * (decimal)num / 100m);
		if (num2 > (decimal)(creature.CurrentHp - 1))
		{
			num2 = creature.CurrentHp - 1;
		}
		if (num2 < 0m)
		{
			num2 = default(decimal);
		}
		if (num2 > 0m)
		{
			int hpLossInt = (int)num2;
			int num3 = TonghuaHealPoolCmd.PeekRemaining(((CardModel)this).Owner);
			DanjinHpBarRenderer.EnterSelfDamage(Math.Min(creature.CurrentHp + num3, creature.MaxHp));
			try
			{
				TonghuaHealPoolCmd.Add(hpLossInt, ((CardModel)this).Owner);
				IEnumerable<DamageResult> obj = await CreatureCmd.Damage(choiceContext, creature, num2, (ValueProp)14, (CardModel)(object)this);
				int num4 = 0;
				foreach (DamageResult item in obj)
				{
					if (item.Receiver == creature)
					{
						num4 += item.UnblockedDamage;
					}
				}
				int num5 = num4 - hpLossInt;
				if (num5 > 0)
				{
					TonghuaHealPoolCmd.Add(num5, ((CardModel)this).Owner);
				}
				else if (num5 < 0)
				{
					TonghuaHealPoolCmd.Subtract(-num5, ((CardModel)this).Owner);
				}
			}
			finally
			{
				DanjinHpBarRenderer.ExitSelfDamage();
				DanjinHpBarRenderer.RequestRefreshFor(((CardModel)this).Owner);
				await TonghuaHealPoolCmd.SyncPower(choiceContext, ((CardModel)this).Owner);
			}
		}
		await PlayerCmd.GainEnergy((decimal)((DynamicVar)((CardModel)this).DynamicVars.Energy).IntValue, ((CardModel)this).Owner);
		if (gongShi >= 1)
		{
			await PowerCmd.Apply<FengMangPower>(choiceContext, creature, ((CardModel)this).DynamicVars["FengMangPower"].BaseValue, creature, (CardModel)(object)this, false);
		}
	}

	protected override void OnUpgrade()
	{
		((DynamicVar)((CardModel)this).DynamicVars.Energy).UpgradeValueBy(1m);
		((CardModel)this).DynamicVars["HpLossPercent"].UpgradeValueBy(-1m);
	}

	private IHoverTip BuildHpLossTip()
	{
		int num = (int)((CardModel)this).DynamicVars["HpLossPercent"].BaseValue;
		Creature val = null;
		try
		{
			Player owner = ((CardModel)this).Owner;
			val = ((owner != null) ? owner.Creature : null);
		}
		catch
		{
		}
		string description;
		if (val != null)
		{
			decimal num2 = Math.Floor((decimal)val.MaxHp * (decimal)num / 100m);
			if (num2 > (decimal)(val.CurrentHp - 1))
			{
				num2 = val.CurrentHp - 1;
			}
			if (num2 < 0m)
			{
				num2 = default(decimal);
			}
			int value = (int)num2;
			description = $"失去[red]{value}[/red]点生命值。";
		}
		else
		{
			description = $"失去最大生命值的[red]{num}%[/red](不致死)。";
		}
		return RawHoverTipBuilder.Build("danjin-jianwu-hploss", "生命值消耗", description);
	}
}
