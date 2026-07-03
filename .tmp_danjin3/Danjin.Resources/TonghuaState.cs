using System;

namespace Danjin.Resources;

public class TonghuaState
{
	private int _value;

	public int Value
	{
		get
		{
			return _value;
		}
		set
		{
			if (_value != value)
			{
				int value2 = _value;
				_value = value;
				this.TonghuaChanged?.Invoke(value2, _value);
			}
		}
	}

	public event Action<int, int>? TonghuaChanged;

	public void GainTonghua(decimal amount)
	{
		if (amount < 0m)
		{
			throw new ArgumentException("Must not be negative.", "amount");
		}
		Value = (int)Math.Max((decimal)Value + amount, 0m);
	}

	public void LoseTonghua(decimal amount)
	{
		if (amount < 0m)
		{
			throw new ArgumentException("Must not be negative.", "amount");
		}
		Value = (int)Math.Max((decimal)Value - amount, 0m);
	}
}
