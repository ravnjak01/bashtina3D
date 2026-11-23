using Bastina3D.Application.DTOs;
using NetTopologySuite.Features;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bastina3D.Application.Interfaces
{
    public interface ISiteService
    {
        Task<FeatureCollection> GetFeatureCollectionAsync(SiteQuery q, CancellationToken ct);
        Task<IReadOnlyList<SiteListItemDto>> ListAsync(SiteQuery q, CancellationToken ct);
        Task<SiteDetailDto?> GetByIdAsync(Guid id, CancellationToken ct);
        Task<SiteDetailDto?> GetBySlugAsync(string slug, CancellationToken ct);
        Task<SiteDetailDto> CreateAsync(CreateSiteRequest req, CancellationToken ct);
    }

}
