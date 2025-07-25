// SPDX-FileCopyrightText: 2025 Starlight
// SPDX-License-Identifier: Starlight-MIT

using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

namespace Content.Server.Database;

public abstract partial class ServerDbBase
{
    public async Task<bool> WriteDbEntryFromComp<TArgs, TEntryData, TComp>(
        EntityManager entityManager,
        Entity<TComp>? entity,
        TArgs queryArgs,
        [RequireStaticDelegate] Func<ServerDbContext, TArgs, Task<(TEntryData? Data, DbSet<TEntryData> DbSet)>> query,
        [RequireStaticDelegate] Func<ServerDbContext, EntityManager, TEntryData, Entity<TComp>, bool> action,
        CancellationToken cancel = default)
        where TEntryData : class, new()
        where TComp: Component
    {
        if (entity == null)
            return false;
        await using var db = await GetDb(cancel);
        var (foundData, foundSet) = (await query(db.DbContext, queryArgs));
        if (cancel.IsCancellationRequested)
            return false;
        if (foundData == null)
        {
            foundData = new TEntryData();
            foundSet.Add(foundData);
        }
        if (!action(db.DbContext, entityManager, foundData, entity.Value))
            return false;
        await db.DbContext.SaveChangesAsync(cancel);
        return true;
    }

    public async Task<bool> ReadDbEntryIntoComp<TArgs, TEntryData, TComp>(
        EntityManager entityManager,
        Entity<TComp>? entity,
        TArgs queryArgs,
        [RequireStaticDelegate] Func<ServerDbContext, TArgs, Task<TEntryData?>> query,
        [RequireStaticDelegate] Func<ServerDbContext, EntityManager, TEntryData, Entity<TComp>, bool> action,
        CancellationToken cancel = default)
        where TEntryData : class
        where TComp: Component
    {
        if (entity == null)
            return false;
        await using var db = await GetDb(cancel);
        var foundData = await query(db.DbContext, queryArgs);
        if (foundData == null || cancel.IsCancellationRequested)
            return false;
        return action(db.DbContext, entityManager, foundData, entity.Value);
    }


    public async Task<bool> ReadFromDbEntry<TArgs, TOutData,TEntryData>(
        TArgs queryArgs,
        TOutData data,
        [RequireStaticDelegate] Func<ServerDbContext, TArgs, Task<TEntryData?>> query,
        [RequireStaticDelegate] Func<ServerDbContext, TEntryData, TOutData, bool> action,
        CancellationToken cancel = default)
        where TEntryData : class
        where TOutData: class
    {
        await using var db = await GetDb(cancel);
        var foundData = await query(db.DbContext, queryArgs);
        if (foundData == null || cancel.IsCancellationRequested)
            return false;
        return action(db.DbContext, foundData, data);
    }

    public async Task<TOutData?> ReadDbEntry<TArgs, TEntryData, TOutData>(
        TArgs queryArgs,
        [RequireStaticDelegate] Func<ServerDbContext, TArgs, Task<TEntryData?>> query,
        [RequireStaticDelegate] Func<ServerDbContext, TEntryData, TOutData> action,
        CancellationToken cancel = default)
        where TArgs : notnull
        where TEntryData: class, new()
    {
        await using var db = await GetDb(cancel);
        var data = await query(db.DbContext, queryArgs);
        if (data == null || cancel.IsCancellationRequested)
            return default;
        return action(db.DbContext, data);
    }

    public async Task<bool> WriteDbEntry<TArgs, TInData, TEntryData>(
        TArgs queryArgs,
        TInData inputData,
        [RequireStaticDelegate] Func<ServerDbContext, TArgs, Task<(TEntryData? Data, DbSet<TEntryData> DbSet)>> query,
        [RequireStaticDelegate] Action<ServerDbContext, TInData, TEntryData> action,
        CancellationToken cancel = default)
        where TArgs : notnull
        where TEntryData: class, new()
    {
        await using var db = await GetDb(cancel);
        var (foundData, foundSet) = (await query(db.DbContext, queryArgs));
        if (cancel.IsCancellationRequested)
            return false;
        if (foundData == null)
        {
            foundData = new TEntryData();
            foundSet.Add(foundData);
        }
        action(db.DbContext, inputData, foundData);
        await db.DbContext.SaveChangesAsync(cancel);
        return true;
    }

    public async Task<bool> DeleteDbEntry<TArgs, TEntryData>(
        TArgs queryArgs,
        [RequireStaticDelegate] Func<ServerDbContext, TArgs, Task<(TEntryData?, DbSet<TEntryData> DbSet)>> query,
        CancellationToken cancel = default)
        where TEntryData : class
    {
        await using var db = await GetDb(cancel);
        var (foundData, foundSet) = await query(db.DbContext, queryArgs);
        if (cancel.IsCancellationRequested || foundData == null)
            return false;
        foundSet.Remove(foundData);
        await db.DbContext.SaveChangesAsync(cancel);
        return true;
    }
}
