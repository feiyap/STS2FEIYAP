using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Danjin.Extensions;
using Danjin.Resources;
using Godot;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Danjin.Powers;

[RegisterPower(Inherit = true)]
public abstract class DanjinPower : ModPowerTemplate
{
	private Func<int, Player, Task>? _gainedHandler;

	private Func<int, Player, Task>? _spentHandler;

	private Action? _removedHandler;

	private bool _subscribed;

	private string LocKeyBase => "DANJIN_POWER_" + StringHelper.Slugify(((object)this).GetType().Name);

	public override LocString Title => new LocString("powers", LocKeyBase + ".title");

	public override LocString Description => new LocString("powers", LocKeyBase + ".description");

	protected override string SmartDescriptionLocKey => LocKeyBase + ".smartDescription";

	private string IconFileBaseName => StringHelper.Slugify(((object)this).GetType().Name).ToLowerInvariant();

	public override string? CustomIconPath
	{
		get
		{
			string text = (IconFileBaseName + ".png").PowerImagePath();
			if (!ResourceLoader.Exists(text, ""))
			{
				return "power.png".PowerImagePath();
			}
			return text;
		}
	}

	public override string? CustomBigIconPath
	{
		get
		{
			string text = (IconFileBaseName + ".png").BigPowerImagePath();
			if (!ResourceLoader.Exists(text, ""))
			{
				return "power.png".BigPowerImagePath();
			}
			return text;
		}
	}

	public virtual Task AfterTonghuaGained(int amount, Player gainer)
	{
		return Task.CompletedTask;
	}

	public virtual Task AfterTonghuaSpent(int amount, Player spender)
	{
		return Task.CompletedTask;
	}

	public override async Task AfterApplied(Creature? applier, CardModel? cardSource)
	{
		SubscribeTonghuaEventsIfNeeded();
		await _003C_003En__0(applier, cardSource);
	}

	private void SubscribeTonghuaEventsIfNeeded()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Invalid comparison between Unknown and I4
		if (!_subscribed && ((PowerModel)this).Owner != null && (int)((PowerModel)this).Owner.Side == 1)
		{
			_gainedHandler = (int amount, Player gainer) => SafeInvokeGained(amount, gainer);
			_spentHandler = (int amount, Player spender) => SafeInvokeSpent(amount, spender);
			TonghuaCmd.AfterTonghuaGained += _gainedHandler;
			TonghuaCmd.AfterTonghuaSpent += _spentHandler;
			_removedHandler = OnPowerRemoved;
			((PowerModel)this).Removed += _removedHandler;
			_subscribed = true;
		}
	}

	private void OnPowerRemoved()
	{
		if (_subscribed)
		{
			if (_gainedHandler != null)
			{
				TonghuaCmd.AfterTonghuaGained -= _gainedHandler;
				_gainedHandler = null;
			}
			if (_spentHandler != null)
			{
				TonghuaCmd.AfterTonghuaSpent -= _spentHandler;
				_spentHandler = null;
			}
			if (_removedHandler != null)
			{
				((PowerModel)this).Removed -= _removedHandler;
				_removedHandler = null;
			}
			_subscribed = false;
		}
	}

	private async Task SafeInvokeGained(int amount, Player gainer)
	{
		try
		{
			await AfterTonghuaGained(amount, gainer);
		}
		catch (Exception value)
		{
			Log.Error($"[Danjin] {((object)this).GetType().Name}.AfterTonghuaGained 异常: {value}", 2);
		}
	}

	private async Task SafeInvokeSpent(int amount, Player spender)
	{
		try
		{
			await AfterTonghuaSpent(amount, spender);
		}
		catch (Exception value)
		{
			Log.Error($"[Danjin] {((object)this).GetType().Name}.AfterTonghuaSpent 异常: {value}", 2);
		}
	}

	[CompilerGenerated]
	[DebuggerHidden]
	private Task _003C_003En__0(Creature? applier, CardModel? cardSource)
	{
		return ((PowerModel)this).AfterApplied(applier, cardSource);
	}
}
