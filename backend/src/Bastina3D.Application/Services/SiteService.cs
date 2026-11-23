// Application/Services/SiteService.cs
using Bastina3D.Application.Abstractions;
using Bastina3D.Application.DTOs;
using Bastina3D.Application.Interfaces;
using Bastina3D.Domain.Entities;
using Bastina3D.Infrastructure;
using Microsoft.EntityFrameworkCore;    // AsNoTracking, ToListAsync, FirstOrDefaultAsync, AnyAsync
using NetTopologySuite.Features;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Bastina3D.Application.Services;

public class SiteService(IAppDbContext db) : ISiteService
{
    public async Task<FeatureCollection> GetFeatureCollectionAsync(SiteQuery q, CancellationToken ct)
    {
        var rows = await Filter(db.Sites.AsNoTracking(), q)
            .Select(s => new
            {
                s.Id,
                s.Name,
                s.Slug,
                s.Category,
                s.YearBuilt,
                s.IsNationalMonument,
                s.HeroImageUrl,
                HasModel = s.Model != null,
                s.Location
            })
            .ToListAsync(ct);

        var fc = new FeatureCollection();
        foreach (var r in rows)
        {
            fc.Add(new Feature(r.Location, new AttributesTable
            {
                { "id", r.Id },
                { "name", r.Name },
                { "slug", r.Slug },
                { "category", r.Category.ToString() },
                { "yearBuilt", r.YearBuilt },
                { "isNationalMonument", r.IsNationalMonument },
                { "hasModel", r.HasModel },
                { "heroImageUrl", r.HeroImageUrl }
            }));
        }
        return fc;
    }

    public async Task<IReadOnlyList<SiteListItemDto>> ListAsync(SiteQuery q, CancellationToken ct)
    {
        var page = Math.Max(q.Page, 1);
        var size = Math.Clamp(q.PageSize, 1, 200);

        return await Filter(db.Sites.AsNoTracking(), q)
            .OrderBy(s => s.Name)
            .Skip((page - 1) * size).Take(size)
            .Select(s => new SiteListItemDto(s.Id, s.Name, s.Slug, s.Category, s.Municipality, s.Location.X, s.Location.Y))
            .ToListAsync(ct);
    }

    public Task<SiteDetailDto?> GetByIdAsync(Guid id, CancellationToken ct) =>
        QueryDetail().Where(s => s.Id == id).FirstOrDefaultAsync(ct);

    public Task<SiteDetailDto?> GetBySlugAsync(string slug, CancellationToken ct) =>
        QueryDetail().Where(s => s.Slug == slug).FirstOrDefaultAsync(ct);

    public async Task<SiteDetailDto> CreateAsync(CreateSiteRequest r, CancellationToken ct)
    {
        var site = new HeritageSite
        {
            Name = r.Name.Trim(),
            NameLocal = r.NameLocal,
            Description = r.Description,
            Category = r.Category,
            Period = r.Period,
            YearBuilt = r.YearBuilt,
            Municipality = r.Municipality,
            IsNationalMonument = r.IsNationalMonument,
            WikidataId = r.WikidataId,
            Location = Geo.CreatePoint(r.Longitude, r.Latitude),
            Slug = await UniqueSlugAsync(r.Name, ct)
        };
        db.Sites.Add(site);
        await db.SaveChangesAsync(ct);
        return (await GetByIdAsync(site.Id, ct))!;
    }

    // ---------- helpers ----------

    private static IQueryable<HeritageSite> Filter(IQueryable<HeritageSite> query, SiteQuery q)
    {
        if (q.Category is { } cat) query = query.Where(s => s.Category == cat);

        if (!string.IsNullOrWhiteSpace(q.Q))
        {
            var term = q.Q.Trim().ToLower();
            query = query.Where(s => s.Name.ToLower().Contains(term) ||
                                     (s.NameLocal != null && s.NameLocal.ToLower().Contains(term)));
        }

        if (TryParseBbox(q.Bbox, out var minLon, out var minLat, out var maxLon, out var maxLat))
            query = query.Where(s => s.Location.X >= minLon && s.Location.X <= maxLon &&
                                     s.Location.Y >= minLat && s.Location.Y <= maxLat);
        return query;
    }

    // bbox = "minLon,minLat,maxLon,maxLat"
    private static bool TryParseBbox(string? bbox, out double minLon, out double minLat, out double maxLon, out double maxLat)
    {
        minLon = minLat = maxLon = maxLat = 0;
        if (string.IsNullOrWhiteSpace(bbox)) return false;
        var p = bbox.Split(',');
        return p.Length == 4
            && double.TryParse(p[0], CultureInfo.InvariantCulture, out minLon)
            && double.TryParse(p[1], CultureInfo.InvariantCulture, out minLat)
            && double.TryParse(p[2], CultureInfo.InvariantCulture, out maxLon)
            && double.TryParse(p[3], CultureInfo.InvariantCulture, out maxLat);
    }

    private IQueryable<SiteDetailDto> QueryDetail() => db.Sites.AsNoTracking().Select(s => new SiteDetailDto(
        s.Id, s.Name, s.NameLocal, s.Slug, s.Description, s.Category, s.Period, s.YearBuilt, s.Municipality,
        s.IsNationalMonument, s.Location.X, s.Location.Y, s.ElevationM, s.WikidataId, s.HeroImageUrl,
        s.Model == null ? null : new ModelDto(s.Model.Url, s.Model.Scale, s.Model.HeadingDeg, s.Model.AltitudeOffsetM, s.Model.FileSizeBytes),
        s.Media.Select(m => new MediaDto(m.Url, m.Caption, m.Attribution, m.License)).ToList()));

    private async Task<string> UniqueSlugAsync(string name, CancellationToken ct)
    {
        var baseSlug = Slugify(name);
        var slug = baseSlug;
        for (var i = 2; await db.Sites.AnyAsync(s => s.Slug == slug, ct); i++) slug = $"{baseSlug}-{i}";
        return slug;
    }

    private static string Slugify(string s)
    {
        var normalized = s.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();
        foreach (var c in normalized)
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark) sb.Append(c);
        var ascii = sb.ToString().ToLowerInvariant().Replace("đ", "dj");
        return Regex.Replace(ascii, "[^a-z0-9]+", "-").Trim('-');
    }
}