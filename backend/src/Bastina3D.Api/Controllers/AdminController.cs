using System.Security.Cryptography;
using System.Text;
using Bastina3D.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Bastina3D.Api.Controllers;


//dosao do pokretanja overpass,baca gresku
[ApiController]
[Route("api/admin")]
public class AdminController(IHeritageImporter importer, IConfiguration config) : ControllerBase
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
}