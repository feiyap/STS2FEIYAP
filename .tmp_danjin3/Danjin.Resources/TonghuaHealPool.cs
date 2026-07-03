using System;

namespace Danjin.Resources;

public class TonghuaHealPool
{
	private int _remaining;

	private int _totalAccumulated;

	public int Remaining => _remaining;

	public int TotalAccumulated => _totalAccumulated;

	public void Add(int amount)
	{
		if (amount > 0)
		{
			_remaining += amount;
			_totalAccumulated += amount;
		}
	}

	public void Subtract(int amount)
	{
		if (amount > 0)
		{
			_remaining = Math.Max(0, _remaining - amount);
		}
	}

	public int TryConsumeForHeal(int desired)
	{
		if (desired <= 0)
		{
			return 0;
		}
		int num = Math.Min(desired, _remaining);
		_remaining -= num;
		return num;
	}
}
