using Bastina3D.Application.Abstractions;
using Bastina3D.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace Bastina3D.Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options),IAppDbContext
{
    public DbSet<HeritageSite> Sites => Set<HeritageSite>();
    public DbSet<SiteModel3D> Models => Set<SiteModel3D>();
    public DbSet<SiteMedia> Media => Set<SiteMedia>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<HeritageSite>(e =>
        {
            e.Property(x => x.Name).HasMaxLength(300).IsRequired();
            e.Property(x => x.NameLocal).HasMaxLength(300);
            e.Property(x => x.Slug).HasMaxLength(200).IsRequired();
            e.Property(x => x.Category).HasConversion<string>().HasMaxLength(40);
            e.Property(x => x.Period).HasMaxLength(100);
            e.Property(x => x.Municipality).HasMaxLength(150);
            e.Property(x => x.WikidataId).HasMaxLength(20);
            e.Property(x => x.OsmType).HasMaxLength(10);

            e.HasIndex(x => x.Slug).IsUnique();
            e.HasIndex(x => x.Category);
            e.HasIndex(x => x.WikidataId);
            e.HasIndex(x => new { x.OsmType, x.OsmId });

            e.HasOne(x => x.Model)
                .WithOne()
                .HasForeignKey<SiteModel3D>(m => m.SiteId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasMany(x => x.Media)
                .WithOne()
                .HasForeignKey(m => m.SiteId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<SiteModel3D>(e =>
        {
            e.Property(x => x.Url).HasMaxLength(1000).IsRequired();
        });

        modelBuilder.Entity<SiteMedia>(e =>
        {
            e.Property(x => x.Url).HasMaxLength(1000).IsRequired();
        });
    }
}