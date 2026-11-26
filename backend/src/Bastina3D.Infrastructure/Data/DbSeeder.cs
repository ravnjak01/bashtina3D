// Data/DbSeeder.cs
using Bastina3D.Common.Domain;
using Bastina3D.Domain.Entities;
using Bastina3D.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Bastina3D.Infrastructure.Data;


public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        if (await db.Sites.AnyAsync()) return;

        db.Sites.AddRange(
            new HeritageSite
            {
                Name = "Stari Most",
                NameLocal = "Stari most",
                Slug = "stari-most",
                Category = SiteCategory.Bridge,
                Period = "Ottoman",
                YearBuilt = 1566,
                Municipality = "Mostar",
                IsNationalMonument = true,
                WikidataId = "Q217100",
                Location = Geo.CreatePoint(17.8150, 43.3372),
                Description = "16th-century Ottoman bridge over the Neretva river."
            },
            new HeritageSite
            {
                Name = "Latin Bridge",
                NameLocal = "Latinska ćuprija",
                Slug = "latin-bridge",
                Category = SiteCategory.Bridge,
                Period = "Ottoman",
                Municipality = "Sarajevo",
                IsNationalMonument = true,
                Location = Geo.CreatePoint(18.4296, 43.8579)
            },
            new HeritageSite
            {
                Name = "Jajce Fortress",
                NameLocal = "Jajačka tvrđava",
                Slug = "jajce-fortress",
                Category = SiteCategory.Fortress,
                Period = "Medieval",
                Municipality = "Jajce",
                IsNationalMonument = true,
                Location = Geo.CreatePoint(17.2703, 44.3429)
            },
            new HeritageSite
            {
                Name = "Počitelj",
                Slug = "pocitelj",
                Category = SiteCategory.OldTown,
                Period = "Ottoman",
                Municipality = "Čapljina",
                IsNationalMonument = true,
                Location = Geo.CreatePoint(17.7219, 43.1322)
            });

        await db.SaveChangesAsync();
    }
}