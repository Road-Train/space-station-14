// SPDX-FileCopyrightText: 2025 Starlight
// SPDX-License-Identifier: Starlight-MIT

using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Robust.Shared.Network;

namespace Content.Server.Database;

public partial interface IServerDbManager
{
    #region Player data
    Task SetPlayerDataForAsync(NetUserId userId, StarLightModel.PlayerDataDTO data, CancellationToken cancel = default);
    Task<StarLightModel.PlayerDataDTO?> GetPlayerDataForAsync(NetUserId userId, CancellationToken cancel = default);
    #endregion
}

public sealed partial class ServerDbManager
{
    public Task<StarLightModel.PlayerDataDTO?> GetPlayerDataForAsync(NetUserId userId, CancellationToken cancel = default)
    {
        DbReadOpsMetric.Inc();
        return RunDbCommand(() => _db.GetPlayerDataDTOForAsync(userId, cancel));
    }
    public Task SetPlayerDataForAsync(NetUserId userId, StarLightModel.PlayerDataDTO data, CancellationToken cancel = default)
    {
        DbReadOpsMetric.Inc();
        return RunDbCommand(() => _db.SetPlayerDataForAsync(userId, data, cancel));
    }


    #region Generic DbEntry Handling
    
    public Task<bool> WriteDbEntryFromCompAsync<TArgs, TEntryData, TComp>(
        EntityManager entityManager,
        Entity<TComp>? entity,
        TArgs queryArgs,
        [RequireStaticDelegate] Func<ServerDbContext, TArgs, Task<(TEntryData? Data, DbSet<TEntryData> DbSet)>> query,
        [RequireStaticDelegate] Func<ServerDbContext, EntityManager, TEntryData, Entity<TComp>, bool> action,
        CancellationToken cancel = default)
        where TEntryData : class, new()
        where TComp: Component
    {
        DbWriteOpsMetric.Inc();
        return RunDbCommand(() => _db.WriteDbEntryFromComp(entityManager, entity, queryArgs, query, action, cancel));
    }

    public Task<bool> ReadDbEntryIntoCompAsync<TArgs, TEntryData, TComp>(
        EntityManager entityManager,
        Entity<TComp>? entity,
        TArgs queryArgs,
        [RequireStaticDelegate] Func<ServerDbContext, TArgs, Task<TEntryData?>> query,
        [RequireStaticDelegate] Func<ServerDbContext, EntityManager, TEntryData, Entity<TComp>, bool> action,
        CancellationToken cancel = default)
        where TEntryData : class
        where TComp: Component
    {
        DbReadOpsMetric.Inc();
        return RunDbCommand(() => _db.ReadDbEntryIntoComp(entityManager, entity, queryArgs, query, action, cancel));
    }


    public Task<bool> ReadFromDbEntryAsync<TArgs, TOutData,TEntryData>(
        TArgs queryArgs,
        TOutData data,
        [RequireStaticDelegate] Func<ServerDbContext, TArgs, Task<TEntryData?>> query,
        [RequireStaticDelegate] Func<ServerDbContext, TEntryData, TOutData, bool> action,
        CancellationToken cancel = default)
        where TEntryData : class
        where TOutData: class
    {
        DbReadOpsMetric.Inc();
        return RunDbCommand(() => _db.ReadFromDbEntry(queryArgs, data, query, action, cancel));
    }

    public Task<TOutData?> ReadDbEntryAsync<TArgs, TEntryData, TOutData>(
        TArgs queryArgs,
        [RequireStaticDelegate] Func<ServerDbContext, TArgs, Task<TEntryData?>> query,
        [RequireStaticDelegate] Func<ServerDbContext, TEntryData, TOutData> action,
        CancellationToken cancel = default)
        where TArgs : notnull
        where TEntryData: class, new()
    {
        DbReadOpsMetric.Inc();
        return RunDbCommand(() => _db.ReadDbEntry(queryArgs, query, action, cancel));
    }

    public Task<bool> WriteDbEntryAsync<TArgs, TInData, TEntryData>(
        TArgs queryArgs,
        TInData inputData,
        [RequireStaticDelegate] Func<ServerDbContext, TArgs, Task<(TEntryData? Data, DbSet<TEntryData> DbSet)>> query,
        [RequireStaticDelegate] Action<ServerDbContext, TInData, TEntryData> action,
        CancellationToken cancel = default)
        where TArgs : notnull
        where TEntryData: class, new()
    {
        DbWriteOpsMetric.Inc();
        return RunDbCommand(() => _db.WriteDbEntry(queryArgs, inputData, query, action, cancel));
    }

    public Task<bool> DeleteDbEntryAsync<TArgs, TEntryData>(
        TArgs queryArgs,
        [RequireStaticDelegate] Func<ServerDbContext, TArgs, Task<(TEntryData?, DbSet<TEntryData> DbSet)>> query,
        CancellationToken cancel = default)
        where TEntryData : class
    {
        DbWriteOpsMetric.Inc();
        return RunDbCommand(() => _db.DeleteDbEntry(queryArgs, query, cancel));
    }


    #endregion
}
