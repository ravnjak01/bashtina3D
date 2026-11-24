// Controllers/SitesController.cs
using Bastina3D.Application.DTOs;
using Bastina3D.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using NetTopologySuite.Features;

namespace BihHeritage.Api.Controllers;

[ApiController]
[Route("api/sites")]
public class SitesController(ISiteService sites) : ControllerBase
{
    /// GeoJSON FeatureCollection, feed directly into a MapLibre `geojson` source.
    [HttpGet("geojson")]
    public async Task<ActionResult<FeatureCollection>> GeoJson([FromQuery] SiteQuery q, CancellationToken ct) =>
        Ok(await sites.GetFeatureCollectionAsync(q, ct));

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<SiteListItemDto>>> List([FromQuery] SiteQuery q, CancellationToken ct) =>
        Ok(await sites.ListAsync(q, ct));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<SiteDetailDto>> GetById(Guid id, CancellationToken ct) =>
        await sites.GetByIdAsync(id, ct) is { } dto ? Ok(dto) : NotFound();

    [HttpGet("by-slug/{slug}")]
    public async Task<ActionResult<SiteDetailDto>> GetBySlug(string slug, CancellationToken ct) =>
        await sites.GetBySlugAsync(slug, ct) is { } dto ? Ok(dto) : NotFound();

    [HttpPost]   // TODO: protect with auth before deploying
    public async Task<ActionResult<SiteDetailDto>> Create(CreateSiteRequest req, CancellationToken ct)
    {
        var created = await sites.CreateAsync(req, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }
}