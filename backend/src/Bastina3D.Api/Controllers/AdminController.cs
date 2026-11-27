using Bastina3D.Application.Abstractions;
using Bastina3D.Application.Import;
using Bastina3D.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace Bastina3D.Api.Controllers;


[ApiController]
[Route("api/admin")]
public class AdminController(IHeritageImporter importer, IConfiguration config,IWikidataEnricher enricher) : ControllerBase
{
    [HttpPost("import/osm")]
    public async Task<IActionResult> ImportOsm([FromHeader(Name = "X-Api-Key")] string? key, CancellationToken ct)
    {
        var expected = config["Admin:ApiKey"];
        if (string.IsNullOrEmpty(expected) || key is null ||
            !CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(key), Encoding.UTF8.GetBytes(expected)))
            return Unauthorized();

        return Ok(await importer.ImportFromOsmAsync(ct));
    }

    [HttpPost("enrich/wikidata")]
    public async Task<IActionResult> EnrichWikidata([FromHeader(Name = "X-Api-Key")] string? key,
    [FromQuery] int max = 200, CancellationToken ct = default)
    {
        if (!Authorized(key)) return Unauthorized();
        return Ok(await enricher.EnrichAsync(Math.Clamp(max, 1, 1000), ct));
    }
    private bool Authorized(string? key)
    {
        var expected = config["Admin:ApiKey"] ?? config["Admin_ApiKey"];
        if (string.IsNullOrEmpty(expected) || string.IsNullOrEmpty(key))
            return false;

        var keyBytes = Encoding.UTF8.GetBytes(key);
        var expectedBytes = Encoding.UTF8.GetBytes(expected);

        return keyBytes.Length == expectedBytes.Length &&
               CryptographicOperations.FixedTimeEquals(keyBytes, expectedBytes);
    }

    // AdminController
    [HttpPost("recompute-rank")]
    public async Task<IActionResult> RecomputeRank([FromHeader(Name = "X-Api-Key")] string? key,
        [FromServices] IAppDbContext db, CancellationToken ct)
    {
        if (!Authorized(key)) return Unauthorized();
        var sites = await db.Sites.Include(s => s.Model).ToListAsync(ct);
        foreach (var s in sites) s.Rank = RankCalculator.Compute(s);
        await db.SaveChangesAsync(ct);
        return Ok(new { updated = sites.Count });
    }
}