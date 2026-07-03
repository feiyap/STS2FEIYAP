using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Danjin.Audio;
using Danjin.Powers;
using Danjin.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;

namespace Danjin.Cards;

public class HuangHuangYouYou : DanjinCard
{
	private static ModSound? _voice;

	protected override HashSet<CardTag> CanonicalTags => new HashSet<CardTag> { (CardTag)0 };

	protected override IEnumerable<DynamicVar> CanonicalVars => Array.Empty<DynamicVar>();

	public HuangHuangYouYou()
		: base(1, (CardType)2, (CardRarity)2, (TargetType)1)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		DanjinVoice.Play(_voice ?? (_voice = new ModSound("res://Danjin/Sounds/Cards/huang_huang_you_you.ogg")));
		Creature creature = ((CardModel)this).Owner.Creature;
		if (creature != null)
		{
			CardModel drawn = (await CardPileCmd.Draw(choiceContext, 1m, ((CardModel)this).Owner, false)).FirstOrDefault();
			if (!creature.HasPower<HuangHuangYouYouPower>())
			{
				await PowerCmd.Apply<HuangHuangYouYouPower>(choiceContext, creature, 1m, creature, (CardModel)(object)this, true);
			}
			HuangHuangYouYouPower power = creature.GetPower<HuangHuangYouYouPower>();
			if (power == null)
			{
				Log.Warn(">>>[DanjinMod] 晃晃悠悠：追踪 Power 创建后仍然取不到，跳过首次效果", 1);
			}
			else if (drawn != null && power.TryConsumeFirstPlay((CardModel)(object)this))
			{
				power.MarkCard(drawn);
			}
			else if (drawn == null)
			{
				Log.Info(">>>[DanjinMod] 晃晃悠悠：没抽到牌，首次标记保留", 2);
			}
		}
	}

	protected override void OnUpgrade()
	{
		((CardModel)this).EnergyCost.UpgradeBy(-1);
	}
}
