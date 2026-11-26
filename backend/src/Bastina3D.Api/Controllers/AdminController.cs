using System.Security.Cryptography;
using System.Text;
using Bastina3D.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

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
}