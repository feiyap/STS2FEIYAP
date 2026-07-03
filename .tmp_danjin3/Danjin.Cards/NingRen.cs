using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Danjin.Character;
using Danjin.Extensions;
using Danjin.Powers;
using Danjin.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.CommonUi;

namespace Danjin.Cards;

public class NingRen : DanjinCard
{
	private static readonly object _poolLock = new object();

	private static List<CardModel>? _feiRenPool;

	protected override HashSet<CardTag> CanonicalTags => new HashSet<CardTag> { (CardTag)0 };

	protected override IEnumerable<IHoverTip> AdditionalHoverTips => new _003C_003Ez__ReadOnlyArray<IHoverTip>((IHoverTip[])(object)new IHoverTip[3]
	{
		HoverTipFactory.FromKeyword(DanjinCardKeywords.FeiRen),
		RawHoverTipBuilder.Build("danjin-stance-atk", "攻势1", "打出时需要至少[blue]1[/blue]层[red]攻势[/red]，否则该效果不生效。"),
		RawHoverTipBuilder.Build("danjin-stance-def", "守势1", "打出时需要至少[blue]1[/blue]层[blue]守势[/blue]，否则该效果不生效。")
	});

	protected override IEnumerable<DynamicVar> CanonicalVars => Array.Empty<DynamicVar>();

	public NingRen()
		: base(1, (CardType)2, (CardRarity)2, (TargetType)1)
	{
	}

	private static List<CardModel> GetFeiRenPool()
	{
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Invalid comparison between Unknown and I4
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Invalid comparison between Unknown and I4
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Invalid comparison between Unknown and I4
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		if (_feiRenPool != null)
		{
			return _feiRenPool;
		}
		lock (_poolLock)
		{
			if (_feiRenPool != null)
			{
				return _feiRenPool;
			}
			List<CardModel> list = new List<CardModel>();
			foreach (CardModel allCard in ModelDb.AllCards)
			{
				if (allCard != null && allCard.Pool is DanjinCardPool)
				{
					CardRarity rarity = allCard.Rarity;
					bool flag = (((int)rarity == 1 || (int)rarity == 5 || (int)rarity == 7) ? true : false);
					if (!flag && allCard is DanjinCard && allCard.Keywords.Contains(DanjinCardKeywords.FeiRen))
					{
						list.Add(allCard);
					}
				}
			}
			_feiRenPool = list.OrderBy<CardModel, string>((CardModel c) => ((AbstractModel)c).Id.Entry, StringComparer.Ordinal).ToList();
			DanjinLog.Verbose($">>>[DanjinMod] 凝刃绯刃池初始化：{list.Count} 张");
			return _feiRenPool;
		}
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ICombatState combatState = ((CardModel)this).CombatState;
		if (combatState == null)
		{
			return;
		}
		Creature creature = ((CardModel)this).Owner.Creature;
		int num = ((creature != null) ? creature.GetPowerAmount<GongShiPower>() : 0);
		Creature creature2 = ((CardModel)this).Owner.Creature;
		int shouShi = ((creature2 != null) ? creature2.GetPowerAmount<ShouShiPower>() : 0);
		List<CardModel> feiRenPool = GetFeiRenPool();
		if (feiRenPool.Count > 0)
		{
			CardModel val = ((CardModel)this).Owner.RunState.Rng.CombatCardGeneration.NextItem<CardModel>((IEnumerable<CardModel>)feiRenPool);
			if (val != null)
			{
				CardModel val2 = null;
				try
				{
					val2 = combatState.CreateCard(val, ((CardModel)this).Owner);
				}
				catch (Exception ex)
				{
					DanjinLog.Verbose(">>>[DanjinMod] 凝刃：创建 " + ((AbstractModel)val).Id.Entry + " 失败：" + ex.Message);
				}
				if (val2 != null)
				{
					if (((CardModel)this).IsUpgraded && val2.IsUpgradable && !val2.IsUpgraded)
					{
						CardCmd.Upgrade(val2, (CardPreviewStyle)1);
					}
					if (num >= 1)
					{
						((CardModel)this).AddKeyword((CardKeyword)1);
						val2.SetToFreeThisTurn();
					}
					CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardToCombat(val2, (PileType)2, ((CardModel)this).Owner, (CardPilePosition)1), 1.2f, (CardPreviewStyle)1);
				}
			}
		}
		if (shouShi < 1)
		{
			return;
		}
		CardPile pile = PileTypeExtensions.GetPile((PileType)2, ((CardModel)this).Owner);
		if (pile != null && pile.Cards.Count > 0)
		{
			CardSelectorPrefs val3 = new CardSelectorPrefs(((CardModel)this).SelectionScreenPrompt, 1);
			CardModel val4 = (await CardSelectCmd.FromHand(choiceContext, ((CardModel)this).Owner, val3, (Func<CardModel, bool>)null, (AbstractModel)(object)this)).FirstOrDefault();
			if (val4 != null)
			{
				await CardPileCmd.Add(val4, (PileType)4, (CardPilePosition)1, (AbstractModel)null, false);
			}
		}
	}

	protected override void OnUpgrade()
	{
	}
}
