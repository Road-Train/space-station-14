// SPDX-FileCopyrightText: 2025 Starlight
// SPDX-License-Identifier: Starlight-MIT

using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Robust.Shared.Network;

// ReSharper disable CheckNamespace

namespace Content.Server.Database;

public partial interface IServerDbManager
{
    #region Player data

    Task SetPlayerDataForAsync(NetUserId userId, StarLightModel.PlayerDataDTO data, CancellationToken cancel = default);
    Task<StarLightModel.PlayerDataDTO?> GetPlayerDataForAsync(NetUserId userId, CancellationToken cancel = default);

    #endregion

    #region Generic DbEntry Handling

    Task<bool> Delete<TResult>([RequireStaticDelegate] Func<ServerDbContext, Task<TResult>> action);

    Task<bool> Delete<T1, TResult>(
        T1 arg1,
        [RequireStaticDelegate] Func<ServerDbContext, T1, Task<TResult>> action);

    Task<bool> Delete<T1, T2, TResult>(
        T1 arg1,
        T2 arg2,
        [RequireStaticDelegate] Func<ServerDbContext, T1, T2, Task<TResult>> action);

    #endregion
}

public sealed partial class ServerDbManager
{
    public Task<StarLightModel.PlayerDataDTO?> GetPlayerDataForAsync(NetUserId userId,
        CancellationToken cancel = default)
    {
        DbReadOpsMetric.Inc();
        return RunDbCommand(() => _db.GetPlayerDataDTOForAsync(userId, cancel));
    }

    public Task SetPlayerDataForAsync(NetUserId userId,
        StarLightModel.PlayerDataDTO data,
        CancellationToken cancel = default)
    {
        DbReadOpsMetric.Inc();
        return RunDbCommand(() => _db.SetPlayerDataForAsync(userId, data, cancel));
    }


    #region Generic DbEntry Handling

    public Task<bool> Delete<TResult>(Func<ServerDbContext, Task<TResult>> action)
    {
        DbWriteOpsMetric.Inc();
        return RunDbCommand(() => _db.Delete(action));
    }

    public Task<bool> Delete<T1, TResult>(
        T1 arg1,
        [RequireStaticDelegate] Func<ServerDbContext, T1, Task<TResult>> action)
    {
        DbWriteOpsMetric.Inc();
        return RunDbCommand(() => _db.Delete(arg1, action));
    }

    public Task<bool> Delete<T1, T2, TResult>(
        T1 arg1,
        T2 arg2,
        [RequireStaticDelegate] Func<ServerDbContext, T1, T2, Task<TResult>> action)
    {
        DbWriteOpsMetric.Inc();
        return RunDbCommand(() => _db.Delete(arg1, arg2, action));
    }

    #endregion
}
