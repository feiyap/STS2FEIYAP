using MegaCrit.Sts2.Core.Entities.Players;

namespace Danjin.Resources;

public static class TonghuaExtensions
{
	public static int GetTonghua(this Player? player)
	{
		if (((player != null) ? player.PlayerCombatState : null) == null)
		{
			return 0;
		}
		return TonghuaCmd.GetState(player.PlayerCombatState).Value;
	}

	public static int GetTonghua(this PlayerCombatState? pcs)
	{
		if (pcs == null)
		{
			return 0;
		}
		return TonghuaCmd.GetState(pcs).Value;
	}

	public static bool HasTonghua(this Player? player, int amount)
	{
		if (player == null || amount < 0)
		{
			return false;
		}
		return player.GetTonghua() >= amount;
	}
}
