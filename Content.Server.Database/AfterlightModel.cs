using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Content.Shared.Database._Afterlight;
using Microsoft.EntityFrameworkCore;

namespace Content.Server.Database;

public abstract partial class ServerDbContext
{
    public DbSet<ALKinks> Kinks { get; set; } = null!;
}

public sealed class AfterlightModel : DataModelBase
{
    public override void OnModelCreating(ServerDbContext dbContext, ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ALKinks>()
            .HasOne(k => k.Player)
            .WithMany(p => p.Kinks)
            .HasForeignKey(k => k.PlayerId)
            .HasPrincipalKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

[Table("al_kinks")]
[PrimaryKey(nameof(PlayerId), nameof(KinkId))]
public sealed class ALKinks
{
    [Key]
    [ForeignKey("Player")]
    public Guid PlayerId { get; set; }

    public Player Player { get; set; } = null!;

    [Key]
    public string KinkId { get; set; } = null!;

    public KinkPreference Preference { get; set; }
}
