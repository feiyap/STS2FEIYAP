using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Danjin.Character;
using Danjin.Extensions;
using Danjin.Patches;
using Danjin.Powers;
using Danjin.Resources;
using Danjin.Utils;
using Danjin.Variables;
using Danjin.Vfx;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Danjin.Cards;

[RegisterCard(typeof(DanjinCardPool), Inherit = true)]
public abstract class DanjinCard : ModCardTemplate, IFeirenHitCountProvider
{
	protected enum SlashAreaMode
	{
		TargetHitbox,
		TargetExpanded,
		FullScreen
	}

	public const decimal DefaultLiaoYuPercentPerStar = 0.03m;

	public override string? CustomFramePath
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_0009: Invalid comparison between Unknown and I4
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_000d: Invalid comparison between Unknown and I4
			CardType type = ((CardModel)this).Type;
			if ((int)type != 1)
			{
				if ((int)type == 3)
				{
					return "res://Danjin/Images/Charui/danjin_jade_frame_power.png";
				}
				return "res://Danjin/Images/Charui/danjin_jade_frame_skill.png";
			}
			return "res://Danjin/Images/Charui/danjin_jade_frame_attack.png";
		}
	}

	public override string CustomPortraitPath
	{
		get
		{
			string text = (IconFileBaseName + ".png").BigCardImagePath();
			if (!ResourceLoader.Exists(text, ""))
			{
				return "card.png".BigCardImagePath();
			}
			return text;
		}
	}

	public override string PortraitPath
	{
		get
		{
			string text = (IconFileBaseName + ".png").CardImagePath();
			if (!ResourceLoader.Exists(text, ""))
			{
				return "card.png".CardImagePath();
			}
			return text;
		}
	}

	public override string BetaPortraitPath
	{
		get
		{
			string text = ("Beta/" + IconFileBaseName + ".png").CardImagePath();
			if (!ResourceLoader.Exists(text, ""))
			{
				return "Beta/card.png".CardImagePath();
			}
			return text;
		}
	}

	private string IconFileBaseName => StringHelper.Slugify(((object)this).GetType().Name).ToLowerInvariant();

	protected bool HasFeiren => ((CardModel)this).Keywords.Contains(DanjinCardKeywords.FeiRen);

	protected virtual decimal FeirenHpLossPerHitPercent => 0.03m;

	protected virtual int FeirenHitCount
	{
		get
		{
			DynamicVar val = default(DynamicVar);
			if (((CardModel)this).DynamicVars.TryGetValue("HitCount", ref val))
			{
				return Math.Max(1, (int)val.BaseValue);
			}
			return 1;
		}
	}

	int IFeirenHitCountProvider.FeirenHitCountForPreview => Math.Max(1, FeirenHitCount);

	public bool IsFeirenHalved { get; private set; }

	protected bool HasLiaoYu => ((CardModel)this).Keywords.Contains(DanjinCardKeywords.LiaoYu);

	protected virtual decimal LiaoYuHealPercentPerStar => 0.03m;

	protected DanjinCard(int cost, CardType type, CardRarity rarity, TargetType target, bool showInCardLibrary = true)
		: base(cost, type, rarity, target, showInCardLibrary)
	{
	}//IL_0002: Unknown result type (might be due to invalid IL or missing references)
	//IL_0003: Unknown result type (might be due to invalid IL or missing references)
	//IL_0004: Unknown result type (might be due to invalid IL or missing references)


	protected virtual int GetFeirenHitCount(Creature? target)
	{
		return FeirenHitCount;
	}

	int IFeirenHitCountProvider.GetFeirenHitCountForPreview(Creature? target)
	{
		return Math.Max(1, GetFeirenHitCount(target));
	}

	protected async Task BeginFeirenAttack(PlayerChoiceContext choiceContext, Creature? target = null)
	{
		IsFeirenHalved = false;
		if (!HasFeiren)
		{
			return;
		}
		Creature creature = ((CardModel)this).Owner.Creature;
		int hits = Math.Max(1, GetFeirenHitCount(target));
		bool chiYunFree = creature != null && creature.HasPower<ChiYunFreeFeirenPower>();
		if (chiYunFree)
		{
			await PowerCmd.Apply<ChiYunFreeFeirenPower>(choiceContext, creature, -1m, creature, (CardModel)null, true);
		}
		if (creature.HasPower<FuSuPower>())
		{
			DanjinLog.Verbose($">>>[DanjinMod][FeiRen] {((CardModel)this).Title}：复苏覆盖，扣血 0，彤华 +{hits}");
			await TonghuaCmd.GainTonghua(hits, ((CardModel)this).Owner);
			return;
		}
		if (chiYunFree)
		{
			DanjinLog.Verbose($">>>[DanjinMod][FeiRen] {((CardModel)this).Title}：赤陨免费，扣血 0，彤华 +{hits}");
			await TonghuaCmd.GainTonghua(hits, ((CardModel)this).Owner);
			return;
		}
		int num = FeiRenVar.CalculateIdealPerHitLoss(creature, FeirenHpLossPerHitPercent);
		int num2 = num * hits;
		int num3 = Math.Max(0, creature.CurrentHp - 1);
		int actualHpLoss;
		if (num2 <= num3)
		{
			actualHpLoss = num2;
			IsFeirenHalved = false;
		}
		else
		{
			actualHpLoss = num3;
			IsFeirenHalved = true;
			DanjinLog.Verbose($">>>[DanjinMod][FeiRen] {((CardModel)this).Title}：HP 不足({creature.CurrentHp}/{creature.MaxHp})，理想扣 {num2}({num}×{hits})，实扣 {actualHpLoss}，本卡效果减半");
		}
		if (actualHpLoss > 0)
		{
			int num4 = TonghuaHealPoolCmd.PeekRemaining(((CardModel)this).Owner);
			DanjinHpBarRenderer.EnterSelfDamage(Math.Min(creature.CurrentHp + num4, creature.MaxHp));
			try
			{
				TonghuaHealPoolCmd.Add(actualHpLoss, ((CardModel)this).Owner);
				IEnumerable<DamageResult> obj = await CreatureCmd.Damage(choiceContext, creature, (decimal)actualHpLoss, (ValueProp)14, (CardModel)(object)this);
				int num5 = 0;
				foreach (DamageResult item in obj)
				{
					if (item.Receiver == creature)
					{
						num5 += item.UnblockedDamage;
					}
				}
				int num6 = num5 - actualHpLoss;
				if (num6 > 0)
				{
					TonghuaHealPoolCmd.Add(num6, ((CardModel)this).Owner);
				}
				else if (num6 < 0)
				{
					TonghuaHealPoolCmd.Subtract(-num6, ((CardModel)this).Owner);
				}
			}
			finally
			{
				DanjinHpBarRenderer.ExitSelfDamage();
				DanjinHpBarRenderer.RequestRefreshFor(((CardModel)this).Owner);
				await TonghuaHealPoolCmd.SyncPower(choiceContext, ((CardModel)this).Owner);
			}
		}
		await TonghuaCmd.GainTonghua(hits, ((CardModel)this).Owner);
	}

	protected int ScaleByFeiren(int value)
	{
		if (!IsFeirenHalved)
		{
			return value;
		}
		if (value <= 0)
		{
			return value;
		}
		return (value + 1) / 2;
	}

	protected decimal ScaleByFeiren(decimal value)
	{
		if (!IsFeirenHalved)
		{
			return value;
		}
		if (value <= 0m)
		{
			return value;
		}
		return Math.Ceiling(value / 2m);
	}

	[Obsolete("旧绯刃机制已废弃，请改用 BeginFeirenAttack() 并配合 ScaleByFeiren()。")]
	protected Task ApplyFeirenHpLoss(PlayerChoiceContext choiceContext)
	{
		return Task.CompletedTask;
	}

	protected Task ApplyLiaoYuHeal(PlayerChoiceContext choiceContext)
	{
		return Task.CompletedTask;
	}

	public override void AfterTransformedFrom()
	{
		((CardModel)this).AfterTransformedFrom();
		Player owner = ((CardModel)this).Owner;
		Creature val = ((owner != null) ? owner.Creature : null);
		if (val != null)
		{
			val.GetPower<MingJingZhiShuiPower>()?.OnCardBeingTransformedAway((CardModel)(object)this);
		}
	}

	protected static void PlayDimensionSlashAll(Creature? target, int lineCount = 3, SlashAreaMode area = SlashAreaMode.TargetExpanded, Color? color = null, float expandDuration = 0.25f, float keepDuration = 0.3f, float contractDuration = 0.3f, float lineFadeIn = 0.2f, float maxLength = 0.75f, float minLength = 0.45f)
	{
		CreateSlash(target, lineCount, area, color, maxLength, minLength, new NDimensionSlashVfx.SlashOptions
		{
			Mode = 0,
			ExpandSlashDuration = expandDuration,
			KeepSlashDuration = keepDuration,
			ContractSlashDuration = contractDuration,
			LineFadeInDuration = lineFadeIn
		})?.TriggerSlash();
	}

	protected static void PlayDimensionSlashSingle(Creature? target, int lineCount = 3, SlashAreaMode area = SlashAreaMode.TargetExpanded, Color? color = null, float expandDuration = 0.2f, float keepDuration = 0.25f, float contractDuration = 0.3f, float eachSlashShownInterval = 0.15f, float lineFadeIn = 0.15f, float maxLength = 0.75f, float minLength = 0.45f)
	{
		CreateSlash(target, lineCount, area, color, maxLength, minLength, new NDimensionSlashVfx.SlashOptions
		{
			Mode = 1,
			ExpandSlashDuration = expandDuration,
			KeepSlashDuration = keepDuration,
			ContractSlashDuration = contractDuration,
			EachSlashShownInterval = eachSlashShownInterval,
			LineFadeInDuration = lineFadeIn
		})?.TriggerSlash();
	}

	protected static NDimensionSlashVfx? PlayDimensionSlashTrigger(Creature? target, int lineCount = 3, SlashAreaMode area = SlashAreaMode.TargetExpanded, Color? color = null, float expandDuration = 0.2f, float keepDuration = 0.2f, float contractDuration = 0.3f, float lineFadeIn = 0.15f, float maxLength = 0.75f, float minLength = 0.45f)
	{
		NDimensionSlashVfx nDimensionSlashVfx = CreateSlash(target, lineCount, area, color, maxLength, minLength, new NDimensionSlashVfx.SlashOptions
		{
			Mode = 2,
			ExpandSlashDuration = expandDuration,
			KeepSlashDuration = keepDuration,
			ContractSlashDuration = contractDuration,
			LineFadeInDuration = lineFadeIn
		});
		if (nDimensionSlashVfx == null)
		{
			return null;
		}
		nDimensionSlashVfx.TriggerSlash();
		return nDimensionSlashVfx;
	}

	protected static NDimensionSlashVfx? PlayDimensionSlashTriggerSingle(Creature? target, int lineCount = 3, SlashAreaMode area = SlashAreaMode.TargetExpanded, Color? color = null, float expandDuration = 0.2f, float keepDuration = 0.2f, float contractDuration = 0.3f, float lineFadeIn = 0.15f, float maxLength = 0.75f, float minLength = 0.45f)
	{
		NDimensionSlashVfx nDimensionSlashVfx = CreateSlash(target, lineCount, area, color, maxLength, minLength, new NDimensionSlashVfx.SlashOptions
		{
			Mode = 3,
			ExpandSlashDuration = expandDuration,
			KeepSlashDuration = keepDuration,
			ContractSlashDuration = contractDuration,
			LineFadeInDuration = lineFadeIn
		});
		if (nDimensionSlashVfx == null)
		{
			return null;
		}
		nDimensionSlashVfx.TriggerSlash();
		return nDimensionSlashVfx;
	}

	private static NDimensionSlashVfx? CreateSlash(Creature? target, int lineCount, SlashAreaMode area, Color? color, float maxLength, float minLength, NDimensionSlashVfx.SlashOptions opts)
	{
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0185: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_019b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d9: Unknown result type (might be due to invalid IL or missing references)
		if (target == null || lineCount <= 0)
		{
			return null;
		}
		NCombatRoom instance = NCombatRoom.Instance;
		if (instance == null)
		{
			return null;
		}
		NCreature creatureNode = instance.GetCreatureNode(target);
		if (creatureNode == null)
		{
			return null;
		}
		float num = (maxLength + minLength) * 0.5f;
		if (num < 0.6f)
		{
			num = 0.6f;
		}
		NDimensionSlashVfx.GenerateConvergingSlashLines(lineCount, num, 0.1f, out var froms, out var tos);
		NDimensionSlashVfx nDimensionSlashVfx = NDimensionSlashVfx.Create(creatureNode, opts, froms, tos);
		if (nDimensionSlashVfx == null)
		{
			return null;
		}
		Rect2 visibleRect;
		switch (area)
		{
		case SlashAreaMode.FullScreen:
		{
			visibleRect = ((Node)creatureNode).GetViewport().GetVisibleRect();
			Vector2 size2 = ((Rect2)(ref visibleRect)).Size;
			nDimensionSlashVfx.SetSlashArea(Vector2.Zero, size2);
			break;
		}
		case SlashAreaMode.TargetExpanded:
		{
			Control hitbox = creatureNode.Hitbox;
			Vector2 val = hitbox.GlobalPosition + hitbox.Size * 0.5f;
			Vector2 val2 = hitbox.Size * 1.6f;
			visibleRect = ((Node)creatureNode).GetViewport().GetVisibleRect();
			Vector2 size = ((Rect2)(ref visibleRect)).Size;
			float num2 = size.X * 0.28f;
			float num3 = size.X * 0.8f;
			float num4 = size.Y * 0.8f;
			((Vector2)(ref val2))._002Ector(Mathf.Clamp(val2.X, num2, num3), Mathf.Clamp(val2.Y, num2, num4));
			float num5 = 50f;
			Vector2 val3 = val - val2 * 0.5f;
			((Vector2)(ref val3))._002Ector(Mathf.Max(val3.X, num5), Mathf.Max(val3.Y, num5));
			if (val3.X + val2.X > size.X - num5)
			{
				val3.X = size.X - num5 - val2.X;
			}
			if (val3.Y + val2.Y > size.Y - num5)
			{
				val3.Y = size.Y - num5 - val2.Y;
			}
			nDimensionSlashVfx.SetSlashArea(val3, val2);
			break;
		}
		}
		if (color.HasValue)
		{
			nDimensionSlashVfx.SetSlashColor(color.Value);
		}
		return nDimensionSlashVfx;
	}

	internal static NDimensionSlashVfx? CreateJianQiSlashAt(Creature? target, int lineCount = 1)
	{
		if (target == null || lineCount <= 0)
		{
			return null;
		}
		return PlayDimensionSlashTriggerSingle(target, lineCount, SlashAreaMode.TargetExpanded, null, 0.15f, 0.3f, 0.4f, 0.08f, 1f, 1f);
	}

	private static (List<(Vector2 center, Vector2 size, NCreature node)>? data, Vector2 vpSize) CollectClampedEnemyData(IReadOnlyList<Creature> enemies, NCombatRoom room)
	{
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_0194: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
		NCreature val = null;
		foreach (Creature enemy in enemies)
		{
			if (enemy != null)
			{
				NCreature creatureNode = room.GetCreatureNode(enemy);
				if (creatureNode != null)
				{
					val = creatureNode;
					break;
				}
			}
		}
		if (val == null)
		{
			return (data: null, vpSize: Vector2.Zero);
		}
		Rect2 visibleRect = ((Node)val).GetViewport().GetVisibleRect();
		Vector2 size = ((Rect2)(ref visibleRect)).Size;
		if (size.X <= 0f || size.Y <= 0f)
		{
			return (data: null, vpSize: Vector2.Zero);
		}
		float num = 16f;
		float num2 = size.X * 0.4f;
		float num3 = size.Y * 0.4f;
		List<(Vector2, Vector2, NCreature)> list = new List<(Vector2, Vector2, NCreature)>();
		Vector2 item = default(Vector2);
		Vector2 item2 = default(Vector2);
		foreach (Creature enemy2 in enemies)
		{
			if (enemy2 == null)
			{
				continue;
			}
			NCreature creatureNode2 = room.GetCreatureNode(enemy2);
			if (creatureNode2 != null)
			{
				Control hitbox = creatureNode2.Hitbox;
				if (!(hitbox.Size.X < num) && !(hitbox.Size.Y < num))
				{
					((Vector2)(ref item))._002Ector(Mathf.Min(hitbox.Size.X, num2), Mathf.Min(hitbox.Size.Y, num3));
					Vector2 val2 = hitbox.GlobalPosition + hitbox.Size * 0.5f;
					((Vector2)(ref item2))._002Ector(Mathf.Clamp(val2.X, size.X * 0.1f, size.X * 0.9f), Mathf.Clamp(val2.Y, size.Y * 0.1f, size.Y * 0.9f));
					list.Add((item2, item, creatureNode2));
				}
			}
		}
		return (data: list, vpSize: size);
	}

	internal static AoeJianQiSlashGroup? CreatePerEnemyJianQiSlashFor(IReadOnlyList<Creature>? enemies, int linesPerEnemy)
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Expected O, but got Unknown
		//IL_02de: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_0193: Unknown result type (might be due to invalid IL or missing references)
		//IL_0195: Unknown result type (might be due to invalid IL or missing references)
		//IL_019f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01be: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_020b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0210: Unknown result type (might be due to invalid IL or missing references)
		//IL_0253: Unknown result type (might be due to invalid IL or missing references)
		//IL_0258: Unknown result type (might be due to invalid IL or missing references)
		if (enemies == null || enemies.Count == 0)
		{
			return null;
		}
		if (linesPerEnemy <= 0)
		{
			return null;
		}
		NCombatRoom instance = NCombatRoom.Instance;
		if (instance == null)
		{
			return null;
		}
		var (list, val) = CollectClampedEnemyData(enemies, instance);
		if (list == null || list.Count == 0)
		{
			return null;
		}
		int num = list.Count;
		if (num > 64)
		{
			num = 64;
		}
		int num2 = Mathf.Max(1, Mathf.Min(linesPerEnemy, 64 / num));
		int num3 = num * num2;
		Vector2[] array = (Vector2[])(object)new Vector2[num3];
		Vector2[] array2 = (Vector2[])(object)new Vector2[num3];
		RandomNumberGenerator val2 = new RandomNumberGenerator();
		val2.Randomize();
		float minAngularSep = (float)Math.PI / (float)(num2 + 2);
		float[] array3 = NDimensionSlashVfx.GenerateBlueNoiseAngles(num, num2, minAngularSep, val2);
		Vector2 val4 = default(Vector2);
		Vector2 val6 = default(Vector2);
		for (int i = 0; i < num2; i++)
		{
			for (int j = 0; j < num; j++)
			{
				(Vector2, Vector2, NCreature) tuple2 = list[j];
				Vector2 item = tuple2.Item1;
				Vector2 item2 = tuple2.Item2;
				Vector2 val3 = new Vector2(item.X / val.X, item.Y / val.Y);
				((Vector2)(ref val4))._002Ector(item2.X / val.X, item2.Y / val.Y);
				float num4 = Mathf.Max(val4.X, val4.Y) * 0.5f;
				float num5 = Mathf.Max(num4 * 2f * 1.6f, 0.08f);
				float num6 = num4 * 0.2f;
				Vector2 val5 = val3 + new Vector2(val2.RandfRange(0f - num6, num6), val2.RandfRange(0f - num6, num6));
				float num7 = array3[i * num + j];
				((Vector2)(ref val6))._002Ector(Mathf.Cos(num7), Mathf.Sin(num7));
				int num8 = i * num + j;
				array[num8] = val5 - val6 * (num5 * 0.5f);
				array2[num8] = val5 + val6 * (num5 * 0.5f);
				array[num8] = new Vector2(Mathf.Clamp(array[num8].X, 0.02f, 0.98f), Mathf.Clamp(array[num8].Y, 0.02f, 0.98f));
				array2[num8] = new Vector2(Mathf.Clamp(array2[num8].X, 0.02f, 0.98f), Mathf.Clamp(array2[num8].Y, 0.02f, 0.98f));
			}
		}
		NDimensionSlashVfx.SlashOptions slashOptions = new NDimensionSlashVfx.SlashOptions();
		slashOptions.Mode = 3;
		slashOptions.ExpandSlashDuration = 0.15f;
		slashOptions.KeepSlashDuration = 0.3f;
		slashOptions.ContractSlashDuration = 0.4f;
		slashOptions.LineFadeInDuration = 0.08f;
		NDimensionSlashVfx.SlashOptions options = slashOptions;
		NDimensionSlashVfx nDimensionSlashVfx = NDimensionSlashVfx.Create(list[0].Item3, options, array, array2);
		if (nDimensionSlashVfx == null)
		{
			return null;
		}
		nDimensionSlashVfx.SetSlashArea(Vector2.Zero, val);
		nDimensionSlashVfx.TriggerSlash();
		return new AoeJianQiSlashGroup(nDimensionSlashVfx, num);
	}

	internal static void PlayPerEnemyJianQiSlashAllAtOnceFor(IReadOnlyList<Creature>? enemies, int linesPerEnemy)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Expected O, but got Unknown
		//IL_02d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_018f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0191: Unknown result type (might be due to invalid IL or missing references)
		//IL_019b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0207: Unknown result type (might be due to invalid IL or missing references)
		//IL_020c: Unknown result type (might be due to invalid IL or missing references)
		//IL_024f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0254: Unknown result type (might be due to invalid IL or missing references)
		if (enemies == null || enemies.Count == 0 || linesPerEnemy <= 0)
		{
			return;
		}
		NCombatRoom instance = NCombatRoom.Instance;
		if (instance == null)
		{
			return;
		}
		var (list, val) = CollectClampedEnemyData(enemies, instance);
		if (list == null || list.Count == 0)
		{
			return;
		}
		int num = list.Count;
		if (num > 64)
		{
			num = 64;
		}
		int num2 = Mathf.Max(1, Mathf.Min(linesPerEnemy, 64 / num));
		int num3 = num * num2;
		Vector2[] array = (Vector2[])(object)new Vector2[num3];
		Vector2[] array2 = (Vector2[])(object)new Vector2[num3];
		RandomNumberGenerator val2 = new RandomNumberGenerator();
		val2.Randomize();
		float minAngularSep = (float)Math.PI / (float)(num2 + 2);
		float[] array3 = NDimensionSlashVfx.GenerateBlueNoiseAngles(num, num2, minAngularSep, val2);
		Vector2 val4 = default(Vector2);
		Vector2 val6 = default(Vector2);
		for (int i = 0; i < num2; i++)
		{
			for (int j = 0; j < num; j++)
			{
				(Vector2, Vector2, NCreature) tuple2 = list[j];
				Vector2 item = tuple2.Item1;
				Vector2 item2 = tuple2.Item2;
				Vector2 val3 = new Vector2(item.X / val.X, item.Y / val.Y);
				((Vector2)(ref val4))._002Ector(item2.X / val.X, item2.Y / val.Y);
				float num4 = Mathf.Max(val4.X, val4.Y) * 0.5f;
				float num5 = Mathf.Max(num4 * 2f * 1.6f, 0.08f);
				float num6 = num4 * 0.2f;
				Vector2 val5 = val3 + new Vector2(val2.RandfRange(0f - num6, num6), val2.RandfRange(0f - num6, num6));
				float num7 = array3[i * num + j];
				((Vector2)(ref val6))._002Ector(Mathf.Cos(num7), Mathf.Sin(num7));
				int num8 = i * num + j;
				array[num8] = val5 - val6 * (num5 * 0.5f);
				array2[num8] = val5 + val6 * (num5 * 0.5f);
				array[num8] = new Vector2(Mathf.Clamp(array[num8].X, 0.02f, 0.98f), Mathf.Clamp(array[num8].Y, 0.02f, 0.98f));
				array2[num8] = new Vector2(Mathf.Clamp(array2[num8].X, 0.02f, 0.98f), Mathf.Clamp(array2[num8].Y, 0.02f, 0.98f));
			}
		}
		NDimensionSlashVfx.SlashOptions slashOptions = new NDimensionSlashVfx.SlashOptions();
		slashOptions.Mode = 0;
		slashOptions.ExpandSlashDuration = 0.18f;
		slashOptions.KeepSlashDuration = 0.32f;
		slashOptions.ContractSlashDuration = 0.4f;
		slashOptions.LineFadeInDuration = 0.1f;
		NDimensionSlashVfx.SlashOptions options = slashOptions;
		NDimensionSlashVfx nDimensionSlashVfx = NDimensionSlashVfx.Create(list[0].Item3, options, array, array2);
		if (nDimensionSlashVfx != null)
		{
			nDimensionSlashVfx.SetSlashArea(Vector2.Zero, val);
			nDimensionSlashVfx.TriggerSlash();
		}
	}

	internal static Func<Task> DoSlashHook(NDimensionSlashVfx? slashVfx)
	{
		return delegate
		{
			slashVfx?.DoSlash();
			return Task.CompletedTask;
		};
	}

	internal static Func<Task> DoSlashHook(AoeJianQiSlashGroup? group)
	{
		return delegate
		{
			group?.DoSlash();
			return Task.CompletedTask;
		};
	}

	protected NDimensionSlashVfx? PrepareJianQiSlash(Creature? target, int lineCount = 1)
	{
		return CreateJianQiSlashAt(target, lineCount);
	}

	protected async Task<AttackCommand?> PlayJianQiAttackOnce(PlayerChoiceContext choiceContext, Creature? target)
	{
		if (target == null)
		{
			return null;
		}
		if (HasFeiren)
		{
			await BeginFeirenAttack(choiceContext, target);
		}
		NDimensionSlashVfx slashVfx = CreateJianQiSlashAt(target);
		slashVfx?.DoSlash();
		AttackCommand result = ((!IsFeirenHalved) ? (await DamageCmd.Attack(((DynamicVar)((CardModel)this).DynamicVars.Damage).BaseValue).FromCard((CardModel)(object)this).Targeting(target)
			.Execute(choiceContext)) : (await DamageCmd.Attack(ScaleByFeiren(((DynamicVar)((CardModel)this).DynamicVars.Damage).BaseValue)).FromCard((CardModel)(object)this).Targeting(target)
			.Execute(choiceContext)));
		slashVfx?.ForceComplete();
		return result;
	}

	protected async Task PlayJianQiAttack(PlayerChoiceContext choiceContext, Creature? target, int hits)
	{
		if (target == null || hits <= 0)
		{
			return;
		}
		if (HasFeiren)
		{
			await BeginFeirenAttack(choiceContext, target);
		}
		NDimensionSlashVfx slashVfx = CreateJianQiSlashAt(target, hits);
		if (target.IsAlive)
		{
			if (!IsFeirenHalved)
			{
				await DamageCmd.Attack(((DynamicVar)((CardModel)this).DynamicVars.Damage).BaseValue).WithHitCount(hits).FromCard((CardModel)(object)this)
					.Targeting(target)
					.BeforeDamage(DoSlashHook(slashVfx))
					.Execute(choiceContext);
			}
			else
			{
				await DamageCmd.Attack(ScaleByFeiren(((DynamicVar)((CardModel)this).DynamicVars.Damage).BaseValue)).WithHitCount(hits).FromCard((CardModel)(object)this)
					.Targeting(target)
					.BeforeDamage(DoSlashHook(slashVfx))
					.Execute(choiceContext);
			}
		}
		slashVfx?.ForceComplete();
	}

	protected AoeJianQiSlashGroup? PrepareAoeJianQiSlashHorizontal()
	{
		ICombatState combatState = ((CardModel)this).CombatState;
		return CreatePerEnemyJianQiSlashFor((combatState != null) ? combatState.HittableEnemies : null, 1);
	}

	protected AoeJianQiSlashGroup? PrepareAoeJianQiSlashRandom(int lineCount)
	{
		ICombatState combatState = ((CardModel)this).CombatState;
		return CreatePerEnemyJianQiSlashFor((combatState != null) ? combatState.HittableEnemies : null, lineCount);
	}

	protected void PlayAoeJianQiSlashAllAtOnce(int lineCount)
	{
		ICombatState combatState = ((CardModel)this).CombatState;
		PlayPerEnemyJianQiSlashAllAtOnceFor((combatState != null) ? combatState.HittableEnemies : null, lineCount);
	}
}
