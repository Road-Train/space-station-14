using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Content.Server.Database;

public partial class Profile
{
    public string Voice { get; set; } = null!;
    public string SiliconVoice { get; set; } = null!;
    public bool HairGlowing { get; set; } = false;
    public bool FacialHairGlowing { get; set; } = false;
    public bool EyeGlowing { get; set; } = false;
    public bool Enabled { get; set; }
    public StarLightModel.StarLightProfile? StarLightProfile { get; set; }

    // public StarLightModel.CharacterInfo? CharacterInfo { get; set; }
}

public abstract partial class ServerDbContext
{
    public DbSet<StarLightModel.PlayerDataDTO> PlayerData { get; set; } = null!;
}

public sealed class StarLightModel : DataModelBase
{
    public override void OnModelCreating(ServerDbContext dbContext, ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<StarLightProfile>(entity =>
        {
            entity.HasOne(e => e.Profile)
                .WithOne(p => p.StarLightProfile)
                .HasForeignKey<StarLightProfile>(e => e.ProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.ProfileId)
                .IsUnique();

            entity.Property(e => e.CustomSpecieName)
                .HasMaxLength(32);
        });
    }

    public class StarLightProfile
    {
        public int Id { get; set; }
        public int ProfileId { get; set; }
        public virtual Profile Profile { get; set; } = null!;
        public string? CustomSpecieName { get; set; }
    }

    [Index(nameof(DiscordId))]
    public class PlayerDataDTO
    {
        [Key] public Guid UserId { get; set; }
        public string? Title { get; set; }
        public string? GhostTheme { get; set; }
        public string? DiscordId { get; set; } = default!;
        public int Balance { get; set; }
        public int Flags { get; set; }
    }
}
