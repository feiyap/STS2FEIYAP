using System.Collections.Generic;
using System.Threading.Tasks;
using Feiyap.Mechanics;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Feiyap.Powers;

/// <summary>
/// 剑圣心：触发居合时获得活力，完美居合时额外获得大量活力。
/// </summary>
[RegisterPower]
public sealed class FeiyapSwordSaintHeartPower : ModPowerTemplate
{
    public const decimal IaidoVigor = 1m;
    public const decimal PerfectIaidoVigor = 5m;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    protected override IEnumerable<string> RegisteredKeywordIds =>
        [FeiyapKeywords.IaidoId, FeiyapKeywords.PerfectIaidoId];

    public static async Task OnIaidoTriggered(
        PlayerChoiceContext choiceContext,
        Creature owner,
        bool perfect)
    {
        var power = owner.GetPower<FeiyapSwordSaintHeartPower>();
        if (power == null)
        {
            return;
        }

        power.Flash();
        await PowerCmd.Apply<VigorPower>(
            choiceContext,
            owner,
            IaidoVigor,
            owner,
            null);

        if (!perfect)
        {
            return;
        }

        power.Flash();
        await PowerCmd.Apply<VigorPower>(
            choiceContext,
            owner,
            PerfectIaidoVigor,
            owner,
            null);
    }
}
