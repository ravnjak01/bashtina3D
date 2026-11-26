using Bastina3D.Application.Abstractions;
using Bastina3D.Application.Import;
using Bastina3D.Application.Interfaces;
using Bastina3D.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Bastina3D.Application.Import;

public class WikidataEnricher(IAppDbContext db, IWikiClient wiki, ILogger<WikidataEnricher> logger) : IWikidataEnricher
{
    public async Task<EnrichResult> EnrichAsync(int maxSites, CancellationToken ct)
    {
        // Only sites with a Wikidata id that still lack a description or image.
        var sites = await db.Sites.Include(s => s.Media)
            .Where(s => s.WikidataId != null && (s.Description == null || s.HeroImageUrl == null))
            .OrderBy(s => s.Name).Take(maxSites).ToListAsync(ct);

        int described = 0, imaged = 0, dated = 0;

        foreach (var batch in sites.Chunk(50))                      // wbgetentities allows 50 ids per call
        {
            var entities = (await wiki.GetEntitiesAsync(batch.Select(s => s.WikidataId!).ToList(), ct))
                .ToDictionary(e => e.Id, StringComparer.OrdinalIgnoreCase);

            foreach (var site in batch)
            {
                if (!entities.TryGetValue(site.WikidataId!, out var e)) continue;

                site.NameLocal ??= e.LabelBs;

                if (site.YearBuilt is null && e.InceptionYear is { } y)
                {
                    site.YearBuilt = y; site.Period ??= OsmTagMapper.PeriodFromYear(y); dated++;
                }

                if (site.Description is null)
                {
                    var text = (e.EnWikiTitle != null ? await wiki.GetSummaryAsync("en", e.EnWikiTitle, ct) : null)
                            ?? (e.BsWikiTitle != null ? await wiki.GetSummaryAsync("bs", e.BsWikiTitle, ct) : null);
                    if (!string.IsNullOrWhiteSpace(text)) { site.Description = text; described++; }
                }

                if (site.HeroImageUrl is null && e.ImageFile != null)
                {
                    var info = await wiki.GetImageInfoAsync(e.ImageFile, ct);
                    var url = $"https://commons.wikimedia.org/wiki/Special:FilePath/{Uri.EscapeDataString(e.ImageFile)}?width=1000";
                    site.HeroImageUrl = url;
                    db.Media.Add(new SiteMedia
                    {
                        SiteId = site.Id,
                        Url = url,
                        Caption = site.Name,
                        Attribution = info.Author,
                        License = info.License
                    });
                    imaged++;
                }
                site.UpdatedAtUtc = DateTime.UtcNow;
            }
          try
            {
                await db.SaveChangesAsync(ct);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                logger.LogWarning(
                    ex,
                    "Concurrency konflikt pri snimanju batcha za Wikidata obogaćivanje. Preskačem problematične zapise iz batcha.");

                foreach (var entry in ex.Entries)
                {
                    entry.State = EntityState.Detached;
                }
            }                         // save per batch: a failure keeps earlier progress
        }
        return new EnrichResult(sites.Count, described, imaged, dated);
    }
}