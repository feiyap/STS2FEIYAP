using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Danjin.Audio;
using Danjin.Powers;
using Danjin.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace Danjin.Cards;

public class DanXinZhaoMingYue : DanjinCard
{
	private static ModSound? _voice;

	public override IEnumerable<CardKeyword> CanonicalKeywords
	{
		get
		{
			if (!((CardModel)this).IsUpgraded)
			{
				return Array.Empty<CardKeyword>();
			}
			return (IEnumerable<CardKeyword>)(object)new CardKeyword[1] { (CardKeyword)3 };
		}
	}

	protected override HashSet<CardTag> CanonicalTags => new HashSet<CardTag> { (CardTag)0 };

	protected override IEnumerable<DynamicVar> CanonicalVars => new _003C_003Ez__ReadOnlySingleElementList<DynamicVar>((DynamicVar)new StarsVar(1));

	public DanXinZhaoMingYue()
		: base(1, (CardType)3, (CardRarity)4, (TargetType)1)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		DanjinVoice.Play(_voice ?? (_voice = new ModSound("res://Danjin/Sounds/Cards/dan_xin_zhao_ming_yue.ogg")));
		int num = (int)((DynamicVar)((CardModel)this).DynamicVars.Stars).BaseValue;
		await PowerCmd.Apply<DanXinZhaoMingYuePower>(choiceContext, ((CardModel)this).Owner.Creature, (decimal)num, ((CardModel)this).Owner.Creature, (CardModel)(object)this, false);
	}

	protected override void OnUpgrade()
	{
		((CardModel)this).AddKeyword((CardKeyword)3);
	}
}
