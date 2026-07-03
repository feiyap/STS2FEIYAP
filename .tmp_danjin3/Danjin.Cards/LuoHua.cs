using System.Collections.Generic;
using System.Threading.Tasks;
using Danjin.Audio;
using Danjin.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Danjin.Cards;

public class LuoHua : DanjinCard
{
	private static ModSound? _voice;

	protected override IEnumerable<IHoverTip> AdditionalHoverTips => new _003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.FromPower<ZhuShiZhiKePower>((int?)null));

	public override int CanonicalStarCost => 2;

	protected override HashSet<CardTag> CanonicalTags => new HashSet<CardTag> { (CardTag)0 };

	protected override IEnumerable<DynamicVar> CanonicalVars => new _003C_003Ez__ReadOnlyArray<DynamicVar>((DynamicVar[])(object)new DynamicVar[2]
	{
		(DynamicVar)new DamageVar(7m, (ValueProp)8),
		(DynamicVar)new PowerVar<ZhuShiZhiKePower>(2m)
	});

	public LuoHua()
		: base(0, (CardType)1, (CardRarity)2, (TargetType)2)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		DanjinVoice.Play(_voice ?? (_voice = new ModSound("res://Danjin/Sounds/Cards/luo_hua.ogg")));
		await PlayJianQiAttackOnce(choiceContext, cardPlay.Target);
		await PowerCmd.Apply<ZhuShiZhiKePower>(choiceContext, cardPlay.Target, ((CardModel)this).DynamicVars["ZhuShiZhiKePower"].BaseValue, ((CardModel)this).Owner.Creature, (CardModel)(object)this, false);
	}

	protected override void OnUpgrade()
	{
		((DynamicVar)((CardModel)this).DynamicVars.Damage).UpgradeValueBy(2m);
		((CardModel)this).DynamicVars["ZhuShiZhiKePower"].UpgradeValueBy(1m);
	}
}
