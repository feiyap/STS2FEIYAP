using System;
using System.Collections.Generic;
using Danjin.Powers;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Combat.HealthBars;

namespace Danjin.Patches;

public class ChuXueHealthForecast : IHealthBarForecastSource
{
	private static readonly Color ForecastColor = new Color("7d144dff");

	public IEnumerable<HealthBarForecastSegment> GetHealthBarForecastSegments(HealthBarForecastContext context)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		Creature creature = ((HealthBarForecastContext)(ref context)).Creature;
		if (creature == null || !CombatManager.Instance.IsInProgress || !creature.IsAlive || creature.IsPlayer)
		{
			yield break;
		}
		WoundPower power = creature.GetPower<WoundPower>();
		if (power == null || ((PowerModel)power).Amount <= 0)
		{
			yield break;
		}
		int num = 2 * ((PowerModel)power).Amount;
		if (num > 0)
		{
			int num2 = Math.Min(num, creature.CurrentHp);
			if (num2 > 0)
			{
				yield return new HealthBarForecastSegment(num2, ForecastColor, (HealthBarForecastGrowthDirection)0, 90);
			}
		}
	}

	public static void Register()
	{
		HealthBarForecastRegistry.Register<ChuXueHealthForecast>("Danjin", "ChuXueForecast");
		Log.Info(">>>[DanjinMod] 伤口 DOT Forecast 注册成功", 2);
	}
}
