using Danjin.Vfx;
using Godot;

namespace Danjin.Cards;

public sealed class AoeJianQiSlashGroup
{
	private readonly NDimensionSlashVfx? _vfx;

	private readonly int _enemyCount;

	public int Count => _enemyCount;

	internal AoeJianQiSlashGroup(NDimensionSlashVfx? vfx, int enemyCount)
	{
		_vfx = vfx;
		_enemyCount = Mathf.Max(1, enemyCount);
	}

	public void DoSlash(int count = 1)
	{
		if (count > 0 && _vfx != null)
		{
			_vfx.DoSlash(count * _enemyCount);
		}
	}

	public void ForceComplete()
	{
		_vfx?.ForceComplete();
	}
}
