using System;
using System.Reflection;
using Danjin.Audio;
using Danjin.Character;
using Danjin.Utils;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;

namespace Danjin.Patches;

[HarmonyPatch]
internal class DanjinHurtVoicePatch
{
	private static ModSound? _hurt1;

	private static ModSound? _hurt2;

	private static ModSound? _critical1;

	private static ModSound? _critical2;

	private static int _lastHurt = -1;

	private static int _lastHurtTurnId = -1;

	private static object? _lastCombatState;

	private static bool _criticalPlayed = false;

	private static readonly Random _rng = new Random();

	private static MethodInfo TargetMethod()
	{
		return AccessTools.Method(AccessTools.TypeByName("MegaCrit.Sts2.Core.Hooks.Hook"), "AfterDamageReceived", (Type[])null, (Type[])null);
	}

	[HarmonyPostfix]
	private static void Postfix(Creature target, DamageResult result, ICombatState combatState)
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			if (combatState == null || result.UnblockedDamage <= 0 || !target.IsPlayer)
			{
				return;
			}
			Player player = target.Player;
			if (!(((player != null) ? player.Character : null) is Danjin.Character.Danjin) || combatState.CurrentSide == target.Side)
			{
				return;
			}
			if (combatState != _lastCombatState)
			{
				_lastCombatState = combatState;
				_criticalPlayed = false;
			}
			if (_lastHurtTurnId == DanjinHurtVoiceTurnReset.TurnId)
			{
				return;
			}
			_lastHurtTurnId = DanjinHurtVoiceTurnReset.TurnId;
			if (!_criticalPlayed && (decimal)target.CurrentHp < (decimal)target.MaxHp * 0.1m)
			{
				_criticalPlayed = true;
				if (_critical1 == null)
				{
					_critical1 = new ModSound("res://Danjin/Sounds/danjin_critical_1.ogg");
				}
				if (_critical2 == null)
				{
					_critical2 = new ModSound("res://Danjin/Sounds/danjin_critical_2.ogg");
				}
				DanjinVoice.Play((_rng.Next(2) == 0) ? _critical1 : _critical2);
				return;
			}
			int num = _lastHurt switch
			{
				0 => 1, 
				1 => 0, 
				_ => _rng.Next(2), 
			};
			if (_hurt1 == null)
			{
				_hurt1 = new ModSound("res://Danjin/Sounds/danjin_hurt_1.ogg");
			}
			if (_hurt2 == null)
			{
				_hurt2 = new ModSound("res://Danjin/Sounds/danjin_hurt_2.ogg");
			}
			DanjinVoice.Play((num == 0) ? _hurt1 : _hurt2);
			_lastHurt = num;
		}
		catch
		{
		}
	}
}
