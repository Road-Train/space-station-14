// SPDX-FileCopyrightText: 2025 Starlight
// SPDX-License-Identifier: Starlight-MIT

using System.Threading.Tasks;

// ReSharper disable CheckNamespace

namespace Content.Server.Database;

public abstract partial class ServerDbBase
{
    public async Task<bool> Delete<TResult>(
        Func<ServerDbContext, Task<TResult>> action)
    {
        await using var db = await GetDb();
        var data = await action(db.DbContext);
        if (data == null)
            return false;

        db.DbContext.Remove(data);
        await db.DbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> Delete<T1, TResult>(
        T1 arg1,
        Func<ServerDbContext, T1, Task<TResult>> action)
    {
        await using var db = await GetDb();
        var data = await action(db.DbContext, arg1);
        if (data == null)
            return false;

        db.DbContext.Remove(data);
        await db.DbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> Delete<T1, T2, TResult>(
        T1 arg1,
        T2 arg2,
        Func<ServerDbContext, T1, T2, Task<TResult>> action)
    {
        await using var db = await GetDb();
        var data = await action(db.DbContext, arg1, arg2);
        if (data == null)
            return false;

        db.DbContext.Remove(data);
        await db.DbContext.SaveChangesAsync();
        return true;
    }
}
