using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Danjin.Cards;
using Danjin.Relics;
using Danjin.Variables;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using STS2RitsuLib.Combat.HealthBars;

namespace Danjin.Patches;

public class FeirenHealthForecast : IHealthBarForecastSource
{
	private static CardModel? _currentHoveredCard;

	private static readonly Color FeirenForecastColor = new Color("7d144dff");

	public IEnumerable<HealthBarForecastSegment> GetHealthBarForecastSegments(HealthBarForecastContext context)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		Creature creature = ((HealthBarForecastContext)(ref context)).Creature;
		if (!creature.IsPlayer || !CombatManager.Instance.IsInProgress)
		{
			yield break;
		}
		CardModel currentHoveredCard = _currentHoveredCard;
		if (currentHoveredCard == null || !currentHoveredCard.Keywords.Contains(DanjinCardKeywords.FeiRen))
		{
			yield break;
		}
		Player owner = currentHoveredCard.Owner;
		if (((owner != null) ? owner.Creature : null) != creature)
		{
			yield break;
		}
		Player player = creature.Player;
		XuePing xuePing = ((player != null) ? player.GetRelic<XuePing>() : null);
		if (xuePing != null && !xuePing.Consumed)
		{
			yield break;
		}
		decimal percent = 0.03m;
		if (currentHoveredCard is DanjinCard)
		{
			FeiRenVar feiRenVar = currentHoveredCard.DynamicVars.Values.OfType<FeiRenVar>().FirstOrDefault();
			if (feiRenVar != null)
			{
				percent = feiRenVar.PerHitPercent;
			}
		}
		int hitCount = ((!(currentHoveredCard is IFeirenHitCountProvider feirenHitCountProvider)) ? 1 : feirenHitCountProvider.GetFeirenHitCountForPreview(null));
		int num = FeiRenVar.CalculateEffectiveTotalHpLoss(creature, percent, hitCount);
		if (num > 0)
		{
			yield return new HealthBarForecastSegment(num, FeirenForecastColor, (HealthBarForecastGrowthDirection)0, 100);
		}
	}

	public static void Register()
	{
		HealthBarForecastRegistry.Register<FeirenHealthForecast>("Danjin", "FeirenForecast");
		Log.Info(">>>[DanjinMod] FeirenForecast注册成功", 2);
	}

	public static void SetHoveredCard(CardModel card)
	{
		_currentHoveredCard = card;
		RefreshOwnerHealthBar();
	}

	public static void ClearHoveredCard()
	{
		_currentHoveredCard = null;
		RefreshOwnerHealthBar();
	}

	private static void RefreshOwnerHealthBar()
	{
		if (!CombatManager.Instance.IsInProgress)
		{
			return;
		}
		NCombatRoom instance = NCombatRoom.Instance;
		if (instance == null)
		{
			return;
		}
		CardModel? currentHoveredCard = _currentHoveredCard;
		object obj;
		if (currentHoveredCard == null)
		{
			obj = null;
		}
		else
		{
			Player owner = currentHoveredCard.Owner;
			obj = ((owner != null) ? owner.Creature : null);
		}
		Creature val = (Creature)obj;
		List<Creature> list = new List<Creature>();
		if (val != null)
		{
			list.Add(val);
		}
		else
		{
			CombatState val2 = CombatManager.Instance.DebugOnlyGetState();
			if (val2 != null)
			{
				foreach (Player player in val2.Players)
				{
					list.Add(player.Creature);
				}
			}
		}
		FieldInfo fieldInfo = AccessTools.Field(typeof(NCreature), "_stateDisplay");
		if (fieldInfo == null)
		{
			return;
		}
		FieldInfo fieldInfo2 = AccessTools.Field(typeof(NCreatureStateDisplay), "_healthBar");
		if (fieldInfo2 == null)
		{
			return;
		}
		foreach (Creature item in list)
		{
			NCreature creatureNode = instance.GetCreatureNode(item);
			if (creatureNode == null)
			{
				continue;
			}
			object? value = fieldInfo.GetValue(creatureNode);
			NCreatureStateDisplay val3 = (NCreatureStateDisplay)((value is NCreatureStateDisplay) ? value : null);
			if (val3 != null)
			{
				object? value2 = fieldInfo2.GetValue(val3);
				object? obj2 = ((value2 is NHealthBar) ? value2 : null);
				if (obj2 != null)
				{
					((NHealthBar)obj2).RefreshValues();
				}
			}
		}
	}
}
