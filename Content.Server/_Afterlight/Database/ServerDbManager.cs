using System.Threading;
using System.Threading.Tasks;
using Content.Shared._Afterlight.Kinks;
using Content.Shared.Database._Afterlight;
using Robust.Shared.Prototypes;

// ReSharper disable CheckNamespace

namespace Content.Server.Database;

public partial class ServerDbManager
{
    #region Kinks

    public Task<List<ALKinks>> GetKinks(Guid player, CancellationToken cancel)
    {
        DbReadOpsMetric.Inc();
        return RunDbCommand(() => _db.GetKinks(player, cancel));
    }

    public Task SetKink(Guid player,
        EntProtoId<KinkDefinitionComponent> kinkId,
        KinkPreference preference,
        CancellationToken cancel)
    {
        DbWriteOpsMetric.Inc();
        return RunDbCommand(() => _db.SetKink(player, kinkId, preference, cancel));
    }

    public Task UpdateKinks(Guid player,
        Dictionary<EntProtoId<KinkDefinitionComponent>, KinkPreference> kinks,
        CancellationToken cancel)
    {
        DbWriteOpsMetric.Inc();
        return RunDbCommand(() => _db.UpdateKinks(player, kinks, cancel));
    }

    public Task UpdateKinks(Guid player,
        IEnumerable<EntProtoId<KinkDefinitionComponent>> kinks,
        KinkPreference preference,
        CancellationToken cancel)
    {
        DbWriteOpsMetric.Inc();
        return RunDbCommand(() => _db.UpdateKinks(player, kinks, preference, cancel));
    }

    public Task RemoveKink(Guid player, EntProtoId<KinkDefinitionComponent> kinkId, CancellationToken cancel)
    {
        DbWriteOpsMetric.Inc();
        return RunDbCommand(() => _db.RemoveKinks(player, kinkId, cancel));
    }

    #endregion
}
