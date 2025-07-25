// SPDX-FileCopyrightText: 2025 Space Kobold Games and Contributors
// SPDX-License-Identifier: MPL-2.0-no-copyleft-exception

using System.Threading;
using System.Threading.Tasks;
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
}
