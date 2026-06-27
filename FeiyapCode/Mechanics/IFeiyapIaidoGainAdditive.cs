namespace Feiyap.Mechanics;

/// <summary>
/// 居合获得量加算修正（如额外 +2 则返回 2m，无修正返回 0m）。
/// </summary>
public interface IFeiyapIaidoGainAdditive
{
    decimal GetIaidoGainAdditiveBonus(in FeiyapIaidoGainContext context);

    /// <summary>居合施加完成后回调；一次性加算源可在此消耗层数。</summary>
    ValueTask OnIaidoGainApplied(FeiyapIaidoGainContext context, decimal appliedBonus)
        => ValueTask.CompletedTask;
}
