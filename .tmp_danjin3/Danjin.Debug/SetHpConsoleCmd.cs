using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.DevConsole;
using MegaCrit.Sts2.Core.DevConsole.ConsoleCommands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;

namespace Danjin.Debug;

public class SetHpConsoleCmd : AbstractConsoleCmd
{
	public override string CmdName => "sethp";

	public override string Args => "<max:int> [target-index:int]";

	public override string Description => "Set creature max HP and heal to full. Index 0=player, 1+=enemies.";

	public override bool IsNetworked => true;

	public override CmdResult Process(Player? issuingPlayer, string[] args)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		if (!CombatManager.Instance.IsInProgress)
		{
			return new CmdResult(false, "Not in combat!");
		}
		if (args.Length < 1 || !int.TryParse(args[0], out var result) || result <= 0)
		{
			return new CmdResult(false, "Usage: sethp <max> [index]");
		}
		CombatState val = CombatManager.Instance.DebugOnlyGetState();
		int result2;
		int num = ((args.Length <= 1 || !int.TryParse(args[1], out result2)) ? 1 : result2);
		if (num < 0 || num >= val.Creatures.Count)
		{
			return new CmdResult(false, $"Invalid index. Range: 0-{val.Creatures.Count - 1}");
		}
		Creature val2 = val.Creatures[num];
		return new CmdResult(DoSetHp(val2, result), true, $"Set {val2} to {result}/{result}");
	}

	private static async Task DoSetHp(Creature creature, int hp)
	{
		await CreatureCmd.SetMaxHp(creature, (decimal)hp);
		await CreatureCmd.Heal(creature, (decimal)hp, true);
	}
}
