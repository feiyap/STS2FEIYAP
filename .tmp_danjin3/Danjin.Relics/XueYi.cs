using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Danjin.Extensions;
using Danjin.Utils;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.Relics;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace Danjin.Relics;

public sealed class XueYi : DanjinRelic
{
	public const int MaxWeakStacks = 6;

	private int _weakStacks = 1;

	[SavedProperty]
	public int WeakStacks
	{
		get
		{
			return _weakStacks;
		}
		set
		{
			int iconTier = GetIconTier(_weakStacks);
			_weakStacks = value;
			if (GetIconTier(_weakStacks) != iconTier)
			{
				RefreshIcon();
			}
			((RelicModel)this).InvokeDisplayAmountChanged();
		}
	}

	public override RelicRarity Rarity => (RelicRarity)6;

	public override bool ShowCounter => true;

	public override int DisplayAmount => WeakStacks;

	private string TierIconName => $"{StringHelper.Slugify(((object)this).GetType().Name).ToLowerInvariant()}_{GetIconTier(_weakStacks)}";

	public override string? CustomIconPath
	{
		get
		{
			string text = (TierIconName + ".png").RelicImagePath();
			if (!ResourceLoader.Exists(text, ""))
			{
				return base.CustomIconPath;
			}
			return text;
		}
	}

	public override string? CustomIconOutlinePath
	{
		get
		{
			string text = (TierIconName + "_outline.png").RelicImagePath();
			if (!ResourceLoader.Exists(text, ""))
			{
				return base.CustomIconOutlinePath;
			}
			return text;
		}
	}

	public override string? CustomBigIconPath
	{
		get
		{
			string text = (TierIconName + ".png").BigRelicImagePath();
			if (!ResourceLoader.Exists(text, ""))
			{
				return base.CustomBigIconPath;
			}
			return text;
		}
	}

	private static int GetIconTier(int stacks)
	{
		if (stacks > 2)
		{
			if (stacks <= 4)
			{
				return 2;
			}
			return 3;
		}
		return 1;
	}

	private void RefreshIcon()
	{
		((RelicModel)this).RelicIconChanged();
		try
		{
			NRun instance = NRun.Instance;
			object obj;
			if (instance == null)
			{
				obj = null;
			}
			else
			{
				NGlobalUi globalUi = instance.GlobalUi;
				if (globalUi == null)
				{
					obj = null;
				}
				else
				{
					NRelicInventory relicInventory = globalUi.RelicInventory;
					obj = ((relicInventory == null) ? null : relicInventory.RelicNodes?.FirstOrDefault((Func<NRelicInventoryHolder, bool>)((NRelicInventoryHolder n) => ((n != null) ? n.Relic : null) != null && (object)n.Relic.Model == this)));
				}
			}
			NRelicInventoryHolder val = (NRelicInventoryHolder)obj;
			if (val != null)
			{
				val.Relic.Model = (RelicModel)(object)this;
			}
		}
		catch (Exception ex)
		{
			DanjinLog.Verbose(">>>[DanjinMod] 血衣: 图标刷新跳过(UI 未就绪): " + ex.Message);
		}
	}

	public override async Task BeforeCombatStart()
	{
		Player owner = ((RelicModel)this).Owner;
		Creature val = ((owner != null) ? owner.Creature : null);
		if (val == null)
		{
			return;
		}
		ICombatState combatState = val.CombatState;
		if (combatState != null)
		{
			List<Creature> list = combatState.HittableEnemies.Where((Creature e) => e.IsAlive).ToList();
			if (list.Count != 0)
			{
				DanjinLog.Verbose($">>>[DanjinMod] 血衣: 战斗开始 → 给予 {list.Count} 个敌人 {WeakStacks} 层虚弱");
				((RelicModel)this).Flash();
				await PowerCmd.Apply<WeakPower>((PlayerChoiceContext)new BlockingPlayerChoiceContext(), (IEnumerable<Creature>)list, (decimal)WeakStacks, val, (CardModel)null, false);
			}
		}
	}

	public override Task AfterCombatEnd(CombatRoom _)
	{
		((RelicModel)this).Status = (RelicStatus)0;
		return Task.CompletedTask;
	}
}
