using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Danjin.Extensions;
using Danjin.Powers;
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

public class SiAnWeiSiTan : DanjinCard
{
	private const decimal HpLossPercent = 0.10m;

	public override IEnumerable<CardKeyword> CanonicalKeywords => new _003C_003Ez__ReadOnlySingleElementList<CardKeyword>((CardKeyword)1);

	protected override HashSet<CardTag> CanonicalTags => new HashSet<CardTag> { (CardTag)0 };

	protected override IEnumerable<IHoverTip> AdditionalHoverTips
	{
		get
		{
			yield return BuildHpLossTip();
			yield return HoverTipFactory.FromPower<SandevistanPower>((int?)null);
		}
	}

	protected override IEnumerable<DynamicVar> CanonicalVars => new _003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new DynamicVar("Stacks", 2m));

	public SiAnWeiSiTan()
		: base(2, (CardType)2, (CardRarity)4, (TargetType)1)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		Creature creature = ((CardModel)this).Owner.Creature;
		int num = Math.Max(1, (int)Math.Floor((decimal)creature.CurrentHp * 0.10m));
		await CreatureCmd.Damage(choiceContext, creature, (decimal)num, (ValueProp)14, (CardModel)(object)this);
		if (creature.IsAlive)
		{
			int num2 = (int)((CardModel)this).DynamicVars["Stacks"].BaseValue;
			await PowerCmd.Apply<SandevistanPower>(choiceContext, creature, (decimal)num2, creature, (CardModel)(object)this, false);
		}
	}

	protected override void OnUpgrade()
	{
		((CardModel)this).DynamicVars["Stacks"].UpgradeValueBy(1m);
	}

	private IHoverTip BuildHpLossTip()
	{
		Player owner = ((CardModel)this).Owner;
		Creature val = ((owner != null) ? owner.Creature : null);
		string description;
		if (val != null)
		{
			int value = Math.Max(1, (int)Math.Floor((decimal)val.CurrentHp * 0.10m));
			description = $"失去[red]{value}[/red]点生命值。";
		}
		else
		{
			description = "失去当前生命值的[red]10%[/red](最少1点)。";
		}
		return RawHoverTipBuilder.Build("danjin-feihongguiying-hploss", "生命值消耗", description);
	}
}
