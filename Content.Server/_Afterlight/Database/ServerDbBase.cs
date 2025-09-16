using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Content.Shared._Afterlight.Kinks;
using Content.Shared.Database._Afterlight;
using Microsoft.EntityFrameworkCore;
using Robust.Shared.Prototypes;

// ReSharper disable CheckNamespace

namespace Content.Server.Database;

public partial class ServerDbBase
{
    #region Afterlight

    public async Task<List<ALKinks>> GetKinks(Guid player, CancellationToken cancel)
    {
        await using var db = await GetDb(cancel);
        return await db.DbContext.Kinks.Where(k => k.PlayerId == player).ToListAsync(cancel);
    }

    public async Task SetKink(Guid player,
        EntProtoId<KinkDefinitionComponent> kinkId,
        KinkPreference preference,
        CancellationToken cancel)
    {
        await using var db = await GetDb(cancel);
        var kink = await db.DbContext.Kinks.FirstOrDefaultAsync(k => k.PlayerId == player && k.KinkId == kinkId.Id,
            cancel);
        kink ??= db.DbContext.Kinks.Add(new ALKinks
            {
                PlayerId = player,
                KinkId = kinkId,
            })
            .Entity;

        kink.Preference = preference;
        await db.DbContext.SaveChangesAsync(cancel);
    }

    public async Task UpdateKinks(Guid player,
        Dictionary<EntProtoId<KinkDefinitionComponent>, KinkPreference> kinks,
        CancellationToken cancel)
    {
        await using var db = await GetDb(cancel);
        foreach (var (kinkId, preference) in kinks)
        {
            var kink = await db.DbContext.Kinks.FirstOrDefaultAsync(k => k.PlayerId == player && k.KinkId == kinkId.Id,
                cancel);
            kink ??= db.DbContext.Kinks.Add(new ALKinks
                {
                    PlayerId = player,
                    KinkId = kinkId
                })
                .Entity;

            kink.Preference = preference;
        }

        await db.DbContext.SaveChangesAsync(cancel);
    }

    public async Task UpdateKinks(Guid player,
        IEnumerable<EntProtoId<KinkDefinitionComponent>> kinks,
        KinkPreference preference,
        CancellationToken cancel)
    {
        await using var db = await GetDb(cancel);
        foreach (var kinkId in kinks)
        {
            var kink = await db.DbContext.Kinks.FirstOrDefaultAsync(k => k.PlayerId == player && k.KinkId == kinkId.Id,
                cancel);
            kink ??= db.DbContext.Kinks.Add(new ALKinks
                {
                    PlayerId = player,
                    KinkId = kinkId
                })
                .Entity;

            kink.Preference = preference;
        }

        await db.DbContext.SaveChangesAsync(cancel);
    }

    public async Task RemoveKinks(Guid player, EntProtoId<KinkDefinitionComponent> kinkId, CancellationToken cancel)
    {
        await using var db = await GetDb(cancel);
        var kink = await db.DbContext.Kinks.FirstOrDefaultAsync(k => k.PlayerId == player && k.KinkId == kinkId.Id,
            cancel);
        if (kink == null)
            return;

        db.DbContext.Kinks.Remove(kink);

        await db.DbContext.SaveChangesAsync(cancel);
    }

    #endregion
}
