using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Danjin.Relics;
using Danjin.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Danjin.Cards;

public class XianZhen : DanjinCard
{
	public override IEnumerable<CardKeyword> CanonicalKeywords => new _003C_003Ez__ReadOnlySingleElementList<CardKeyword>((CardKeyword)1);

	protected override HashSet<CardTag> CanonicalTags => new HashSet<CardTag> { (CardTag)0 };

	protected override IEnumerable<IHoverTip> AdditionalHoverTips
	{
		get
		{
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			XueYi xueYi = ModelDb.Relic<XueYi>();
			return new _003C_003Ez__ReadOnlyArray<IHoverTip>((IHoverTip[])(object)new IHoverTip[2]
			{
				HoverTipFactory.Static((StaticHoverTip)6, Array.Empty<DynamicVar>()),
				(IHoverTip)(object)new HoverTip(((RelicModel)xueYi).Title, ((RelicModel)xueYi).DynamicDescription, ((RelicModel)xueYi).Icon)
			});
		}
	}

	protected override IEnumerable<DynamicVar> CanonicalVars => new _003C_003Ez__ReadOnlySingleElementList<DynamicVar>((DynamicVar)new DamageVar(10m, (ValueProp)8));

	public XianZhen()
		: base(1, (CardType)1, (CardRarity)4, (TargetType)2)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, "Target");
		bool shouldTriggerFatal = cardPlay.Target.Powers.All((PowerModel p) => p.ShouldOwnerDeathTriggerFatal());
		AttackCommand val = await PlayJianQiAttackOnce(choiceContext, cardPlay.Target);
		if (!shouldTriggerFatal || val == null || !val.Results.SelectMany((List<DamageResult> r) => r).Any((DamageResult r) => r.WasTargetKilled))
		{
			return;
		}
		XueYi relic = ((CardModel)this).Owner.GetRelic<XueYi>();
		if (relic != null)
		{
			if (relic.WeakStacks < 6)
			{
				relic.WeakStacks++;
			}
			DanjinLog.Verbose($">>>[DanjinMod] 陷阵：斩杀！血衣虚弱层数 → {relic.WeakStacks}");
		}
		else
		{
			await RelicCmd.Obtain<XueYi>(((CardModel)this).Owner);
			DanjinLog.Verbose(">>>[DanjinMod] 陷阵：斩杀！获得血衣遗物");
		}
	}

	protected override void OnUpgrade()
	{
		((DynamicVar)((CardModel)this).DynamicVars.Damage).UpgradeValueBy(5m);
	}
}
