using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TreasureHuntApp.Core.Entities;

namespace TreasureHuntApp.Infrastructure.Data;
public class TreasureHuntDbContext(DbContextOptions<TreasureHuntDbContext> options)
    : IdentityDbContext<UserEntity>(options)
{
    public DbSet<EventEntity> Events { get; set; }
    public DbSet<TeamEntity> Teams { get; set; }
    public DbSet<LocationEntity> Locations { get; set; }
    public DbSet<VisitEntity> Visits { get; set; }
    public DbSet<PhotoEntity> Photos { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        ConfigureEvents(builder);
        ConfigureTeams(builder);
        ConfigureLocations(builder);
        ConfigureVisits(builder);
        ConfigurePhotos(builder);
    }

    private static void ConfigureEvents(ModelBuilder builder)
    {
        builder.Entity<EventEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Type).HasConversion<string>();
            entity.Property(e => e.Status).HasConversion<string>();

            // Index for active events
            entity.HasIndex(e => new { e.Status, e.StartTime });
        });
    }

    private static void ConfigureTeams(ModelBuilder builder)
    {
        builder.Entity<TeamEntity>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.Property(t => t.Name).IsRequired().HasMaxLength(100);
            entity.Property(t => t.AccessCode).IsRequired().HasMaxLength(10);

            // Unique constraint: AccessCode + EventId
            entity.HasIndex(t => new { t.AccessCode, t.EventId }).IsUnique();

            // Foreign key to Event
            entity.HasOne(t => t.Event)
                  .WithMany(e => e.Teams)
                  .HasForeignKey(t => t.EventId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureLocations(ModelBuilder builder)
    {
        builder.Entity<LocationEntity>(entity =>
        {
            entity.HasKey(l => l.Id);
            entity.Property(l => l.Name).IsRequired().HasMaxLength(200);
            entity.Property(l => l.Address).IsRequired().HasMaxLength(500);
            entity.Property(l => l.Latitude).HasPrecision(10, 8);
            entity.Property(l => l.Longitude).HasPrecision(11, 8);

            // Spatial index for GPS coordinates
            entity.HasIndex(l => new { l.Latitude, l.Longitude });

            // Foreign key to Event
            entity.HasOne(l => l.Event)
                  .WithMany(e => e.Locations)
                  .HasForeignKey(l => l.EventId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureVisits(ModelBuilder builder)
    {
        builder.Entity<VisitEntity>(entity =>
        {
            entity.HasKey(v => v.Id);

            // Unique constraint: one visit per team per location
            entity.HasIndex(v => new { v.TeamId, v.LocationId }).IsUnique();

            // Foreign keys
            entity.HasOne(v => v.Team)
                  .WithMany(t => t.Visits)
                  .HasForeignKey(v => v.TeamId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(v => v.Location)
                  .WithMany(l => l.Visits)
                  .HasForeignKey(v => v.LocationId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigurePhotos(ModelBuilder builder)
    {
        builder.Entity<PhotoEntity>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.FileName).IsRequired().HasMaxLength(255);
            entity.Property(p => p.FilePath).IsRequired().HasMaxLength(500);

            // Foreign keys
            entity.HasOne(p => p.Team)
                  .WithMany(t => t.Photos)
                  .HasForeignKey(p => p.TeamId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(p => p.Location)
                  .WithMany()
                  .HasForeignKey(p => p.LocationId)
                  .OnDelete(DeleteBehavior.Restrict); // Don't delete photos when location deleted
        });
    }
}