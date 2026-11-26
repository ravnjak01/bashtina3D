using System.Text.RegularExpressions;
using Bastina3D.Application.Abstractions;
using Bastina3D.Application.Common;
using Bastina3D.Application.Interfaces;
using Bastina3D.Domain.Entities;
using Bastina3D.Domain.Enums;
using Bastina3D.Domain;
using Microsoft.EntityFrameworkCore;
using Bastina3D.Common.Domain;

namespace Bastina3D.Application.Import;

public partial class OsmImporter(IAppDbContext db, IOverpassClient overpass) : IHeritageImporter
{
    private const int MaxNameLength = 300;
    private const int MaxSlugLength = 200;
    private const int MaxMunicipalityLength = 150;

    private static readonly Regex WikidataIdRegex = new(
        @"^Q\d{1,15}$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public async Task<ImportResult> ImportFromOsmAsync(CancellationToken ct)
    {
        var elements = await overpass.FetchHeritageElementsAsync(ct);

        var existing = await db.Sites.ToListAsync(ct);
        var byOsm = new Dictionary<(string Type, long Id), HeritageSite>();
        var byWikidata = new Dictionary<string, HeritageSite>(StringComparer.OrdinalIgnoreCase);
        var slugs = new HashSet<string>(
            existing.Where(s => !string.IsNullOrWhiteSpace(s.Slug)).Select(s => s.Slug),
            StringComparer.OrdinalIgnoreCase);

        foreach (var s in existing)
        {
            if (!string.IsNullOrWhiteSpace(s.OsmType) && s.OsmId is not null)
                byOsm.TryAdd((s.OsmType, s.OsmId.Value), s);

            if (!string.IsNullOrWhiteSpace(s.WikidataId))
                byWikidata.TryAdd(s.WikidataId, s);
        }

        int created = 0, updated = 0, skipped = 0;

        foreach (var el in elements)
        {
            ct.ThrowIfCancellationRequested();

            var name = OsmTagMapper.GetName(el.Tags)?.Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                skipped++;
                continue;
            }

            if (!IsValidWgs84(el.Lat, el.Lon))
            {
                skipped++;
                continue;
            }

            el.Tags.TryGetValue("wikidata", out var rawWikidata);
            var wikidata = !string.IsNullOrWhiteSpace(rawWikidata) && WikidataIdRegex.IsMatch(rawWikidata.Trim())
                ? rawWikidata.Trim().ToUpperInvariant()
                : null;

            byOsm.TryGetValue((el.Type, el.Id), out var site);
            if (site is null && wikidata is not null)
                byWikidata.TryGetValue(wikidata, out site);

            var year = OsmTagMapper.ParseYear(el.Tags);
            var municipality = Truncate(OsmTagMapper.GetMunicipality(el.Tags)?.Trim(), MaxMunicipalityLength);
            var mappedCategory = OsmTagMapper.MapCategory(el.Tags);
            var location = Geo.CreatePoint(el.Lon, el.Lat); // (lon, lat) - SRID 4326

            if (site is null)
            {
                var slug = Truncate(SlugGenerator.Unique(name, slugs), MaxSlugLength)!;
                slugs.Add(slug);

                site = new HeritageSite
                {
                    Name = Truncate(name, MaxNameLength)!,
                    NameLocal = Truncate(OsmTagMapper.GetLocalName(el.Tags)?.Trim(), MaxNameLength),
                    Slug = slug,
                    Category = mappedCategory,
                    YearBuilt = year,
                    Period = OsmTagMapper.PeriodFromYear(year),
                    Municipality = municipality,
                    Location = location,
                    WikidataId = wikidata,
                    OsmType = el.Type,
                    OsmId = el.Id,
                    CreatedAtUtc = DateTime.UtcNow,
                    UpdatedAtUtc = DateTime.UtcNow
                };

                db.Sites.Add(site);
                byOsm[(el.Type, el.Id)] = site;
                if (wikidata is not null)
                    byWikidata.TryAdd(wikidata, site);

                created++;
            }
            else
            {
                // Nikad ne prepisuj ručno uređene opise/nazive; samo unaprijedi geometriju i popuni prazna polja.
                site.Location = location;
                site.OsmType ??= el.Type;
                site.OsmId ??= el.Id;
                site.WikidataId ??= wikidata;
                site.YearBuilt ??= year;
                site.Period ??= OsmTagMapper.PeriodFromYear(site.YearBuilt);
                site.Municipality ??= municipality;

                if (site.Category == SiteCategory.Other && mappedCategory != SiteCategory.Other)
                    site.Category = mappedCategory;

                if (wikidata is not null)
                    byWikidata.TryAdd(wikidata, site);

                site.UpdatedAtUtc = DateTime.UtcNow;
                updated++;
            }
        }

        await db.SaveChangesAsync(ct);
        return new ImportResult(elements.Count, created, updated, skipped);
    }

    private static bool IsValidWgs84(double lat, double lon) =>
        !double.IsNaN(lat) && !double.IsInfinity(lat) &&
        !double.IsNaN(lon) && !double.IsInfinity(lon) &&
        lat is >= -90.0 and <= 90.0 &&
        lon is >= -180.0 and <= 180.0;

    private static string? Truncate(string? value, int maxLength) =>
        string.IsNullOrEmpty(value) || value.Length <= maxLength
            ? value
            : value[..maxLength];
}