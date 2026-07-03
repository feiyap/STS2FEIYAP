using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Danjin.Cards;
using Danjin.Extensions;
using Danjin.Relics;
using Godot;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Characters;

namespace Danjin.Character;

[RegisterCharacter]
public class Danjin : ModCharacterTemplate<DanjinCardPool, DanjinRelicPool, DanjinPotionPool>
{
	public const string CharacterId = "Danjin";

	public static readonly Color Color = new Color("f60059ff");

	public override Color NameColor => Color;

	public override Color EnergyLabelOutlineColor => new Color("06414aff");

	public override CharacterGender Gender => (CharacterGender)1;

	public override int StartingHp => 75;

	public override Color MapDrawingColor => new Color("6d0032ff");

	public override bool ShouldAlwaysShowStarCounter => false;

	public override bool RequiresEpochAndTimeline => false;

	public override int StartingGold => 99;

	public override float AttackAnimDelay => 0.15f;

	public override float CastAnimDelay => 0.25f;

	protected override IEnumerable<CardModel> LocalStartingDeck => new _003C_003Ez__ReadOnlyArray<CardModel>((CardModel[])(object)new CardModel[10]
	{
		(CardModel)ModelDb.Card<BasicAttack>(),
		(CardModel)ModelDb.Card<BasicAttack>(),
		(CardModel)ModelDb.Card<BasicAttack>(),
		(CardModel)ModelDb.Card<BasicAttack>(),
		(CardModel)ModelDb.Card<BasicBlock>(),
		(CardModel)ModelDb.Card<BasicBlock>(),
		(CardModel)ModelDb.Card<BasicBlock>(),
		(CardModel)ModelDb.Card<BasicBlock>(),
		(CardModel)ModelDb.Card<JianQi>(),
		(CardModel)ModelDb.Card<ZhuiLie>()
	});

	protected override IEnumerable<RelicModel> LocalStartingRelics => new _003C_003Ez__ReadOnlySingleElementList<RelicModel>((RelicModel)(object)ModelDb.Relic<HuaiJinZhiYu>());

	public override string? CustomVisualsPath => "res://Danjin/Scenes/danjin.tscn";

	public override string? CustomCharacterSelectBgPath => "res://Danjin/Scenes/danjin_portrait.tscn";

	public override string? CustomIconTexturePath => "danjin_icon.png".CharacterUiPath();

	public override string? CustomIconOutlineTexturePath => "danjin_icon.png".CharacterUiPath();

	public override string? CustomIconPath => "res://Danjin/Scenes/danjin_icon.tscn";

	public override string? CustomCharacterSelectIconPath => "char_select_char_name.png".CharacterUiPath();

	public override string? CustomCharacterSelectLockedIconPath => "char_select_char_name_locked.png".CharacterUiPath();

	public override string? CustomMapMarkerPath => "map_marker_char_name.png".CharacterUiPath();

	public override string? CustomEnergyCounterPath => "res://Danjin/Scenes/danjin_energy_counter.tscn";

	public override string? CustomMerchantAnimPath => "res://Danjin/Scenes/danjin_merchant.tscn";

	public override string? CustomArmPointingTexturePath => "danjin_arm_point.png".CharacterUiPath();

	public override string? CustomArmRockTexturePath => "danjin_arm_rock.png".CharacterUiPath();

	public override string? CustomArmPaperTexturePath => "danjin_arm_paper.png".CharacterUiPath();

	public override string? CustomArmScissorsTexturePath => "danjin_arm_scissors.png".CharacterUiPath();

	public override string? CustomTrailPath => SceneHelper.GetScenePath("vfx/card_trail_silent");

	public override string? CustomRestSiteAnimPath => SceneHelper.GetScenePath("rest_site/characters/silent_rest_site");

	public override string? CustomCharacterSelectTransitionPath => "res://materials/transitions/silent_transition_mat.tres";

	public override string? CustomCharacterSelectSfx => "";

	public override string? CustomCharacterTransitionSfx => "event:/sfx/ui/wipe_silent";

	public override string? CustomAttackSfx => "event:/sfx/characters/silent/silent_attack";

	public override string? CustomCastSfx => "event:/sfx/characters/silent/silent_cast";

	public override string? CustomDeathSfx => "event:/sfx/characters/silent/silent_die";

	public override List<string> GetArchitectAttackVfx()
	{
		int num = 5;
		List<string> list = new List<string>(num);
		CollectionsMarshal.SetCount(list, num);
		Span<string> span = CollectionsMarshal.AsSpan(list);
		span[0] = "vfx/vfx_attack_blunt";
		span[1] = "vfx/vfx_heavy_blunt";
		span[2] = "vfx/vfx_attack_slash";
		span[3] = "vfx/vfx_bloody_impact";
		span[4] = "vfx/vfx_rock_shatter";
		return list;
	}
}
