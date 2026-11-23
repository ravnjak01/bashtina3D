using Bastina3D.Domain.Entities;
using BihHeritage.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Bastina3D.Application
{
    public class SiteHelper
    {

        private static IQueryable<HeritageSite> Filter(IQueryable<HeritageSite> query, SiteQuery q)
        {
            if (q.Category is { } cat) query = query.Where(s => s.Category == cat);

            if (!string.IsNullOrWhiteSpace(q.Q))
            {
                var term = q.Q.Trim().ToLower();   // ToLower() keeps it portable (SQL Server CI vs Postgres CS collations)
                query = query.Where(s => s.Name.ToLower().Contains(term) ||
                                         (s.NameLocal != null && s.NameLocal.ToLower().Contains(term)));
            }

            if (TryParseBbox(q.Bbox, out var minLon, out var minLat, out var maxLon, out var maxLat))
                query = query.Where(s => s.Location.X >= minLon && s.Location.X <= maxLon &&
                                         s.Location.Y >= minLat && s.Location.Y <= maxLat);
            return query;
        }

        // bbox = "minLon,minLat,maxLon,maxLat" (same order as MapLibre's map.getBounds().toArray().flat())
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
            var normalized = s.Normalize(NormalizationForm.FormD);       // strips č/ć/š/ž/đ diacritics
            var sb = new StringBuilder();
            foreach (var c in normalized)
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark) sb.Append(c);
            var ascii = sb.ToString().ToLowerInvariant().Replace("đ", "dj");
            return Regex.Replace(ascii, "[^a-z0-9]+", "-").Trim('-');
        }
    }
}
