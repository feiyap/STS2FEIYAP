namespace Feiyap.Mechanics;

/// <summary>
/// 居合获得量乘算修正（如提升 25% 则返回 amount * 1.25m）。
/// </summary>
public interface IFeiyapIaidoGainMultiplier
{
    decimal ModifyIaidoGainMultiplicative(in FeiyapIaidoGainContext context, decimal amount);
}
