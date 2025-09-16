using System.Threading;
using System.Threading.Tasks;
using Content.Shared._Afterlight.Kinks;
using Content.Shared.Database._Afterlight;
using Robust.Shared.Prototypes;

// ReSharper disable CheckNamespace

namespace Content.Server.Database;

public partial interface IServerDbManager
{
    #region Kinks

    Task<List<ALKinks>> GetKinks(Guid player, CancellationToken cancel);

    Task SetKink(Guid player,
        EntProtoId<KinkDefinitionComponent> kinkId,
        KinkPreference preference,
        CancellationToken cancel);

    Task UpdateKinks(Guid player,
        Dictionary<EntProtoId<KinkDefinitionComponent>, KinkPreference> kinks,
        CancellationToken cancel);

    Task UpdateKinks(Guid player,
        IEnumerable<EntProtoId<KinkDefinitionComponent>> kinks,
        KinkPreference preference,
        CancellationToken cancel);

    Task RemoveKink(Guid player, EntProtoId<KinkDefinitionComponent> kinkId, CancellationToken cancel);

    #endregion
}
