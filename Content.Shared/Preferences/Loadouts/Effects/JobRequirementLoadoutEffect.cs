using System.Diagnostics.CodeAnalysis;
using Content.Shared._Afterlight.CCVar;
using Content.Shared.Players.PlayTimeTracking;
using Content.Shared.Roles;
using Robust.Shared.Configuration;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared.Preferences.Loadouts.Effects;

/// <summary>
/// Checks for a job requirement to be met such as playtime.
/// </summary>
public sealed partial class JobRequirementLoadoutEffect : LoadoutEffect
{
    private static IConfigurationManager ConfigurationManager => IoCManager.Resolve<IConfigurationManager>(); // afterlight

    
    [DataField(required: true)]
    public JobRequirement Requirement = default!;

    public override bool Validate(HumanoidCharacterProfile profile, RoleLoadout loadout, ICommonSession? session, IDependencyCollection collection, [NotNullWhen(false)] out FormattedMessage? reason)
    {
        if (session == null)
        {
            reason = FormattedMessage.Empty;
            return true;
        }

        var manager = collection.Resolve<ISharedPlaytimeManager>();
        var playtimes = manager.GetPlayTimes(session);

        if (!ConfigurationManager.GetCVar(ALCVars.ALUseLoadoutRequirements)) // afterlight
        {
            reason = FormattedMessage.Empty;
            return true;
        }
        
        return Requirement.Check(collection.Resolve<IEntityManager>(),
            session,
            collection.Resolve<IPrototypeManager>(),
            profile,
            playtimes,
            out reason);
    }
}
