using System;
using System.Collections.Generic;
using System.Text;

namespace Bastina3D.Application.Interfaces
{
    public record WikidataEntity(string Id, string? LabelEn, string? LabelBs, string? ImageFile,
    int? InceptionYear, string? EnWikiTitle, string? BsWikiTitle);

    public record ImageInfo(string? Author, string? License);
    public interface IWikiClient
    {
        Task<IReadOnlyList<WikidataEntity>> GetEntitiesAsync(IReadOnlyCollection<string> ids, CancellationToken ct);
        Task<string?> GetSummaryAsync(string lang, string title, CancellationToken ct);
        Task<ImageInfo> GetImageInfoAsync(string fileName, CancellationToken ct);
    }

    public record EnrichResult(int Processed, int Described, int Imaged, int Dated);

    public interface IWikidataEnricher
    {
        Task<EnrichResult> EnrichAsync(int maxSites, CancellationToken ct);
    }
}
