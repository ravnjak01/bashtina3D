// Infrastructure/Import/OverpassClient.cs
using System.Net;
using System.Text.Json;
using Bastina3D.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace Bastina3D.Infrastructure.Import;

public class OverpassClient(HttpClient http, ILogger<OverpassClient> log) : IOverpassClient
{
    private static readonly string[] Endpoints =
    [
        "https://overpass-api.de/api/interpreter",
        "https://overpass.private.coffee/api/interpreter",
        "https://overpass.kumi.systems/api/interpreter",
    ];

    private const string Query = """
        [out:json][timeout:180];
        area["ISO3166-1"="BA"]->.ba;
        (
          nwr["historic"~"^(castle|fort|city_gate|citywalls|tower|monument|memorial|archaeological_site|ruins|tomb|cemetery|monastery|bridge|wayside_cross|wayside_shrine)$"]["name"](area.ba);
          nwr["heritage"]["name"](area.ba);
        );
        out center tags;
        """;

    public async Task<IReadOnlyList<OsmElement>> FetchHeritageElementsAsync(CancellationToken ct)
    {
        Exception? last = null;

        for (var attempt = 0; attempt < 6; attempt++)
        {
            var url = Endpoints[attempt % Endpoints.Length];
            try
            {
                using var content = new FormUrlEncodedContent([new KeyValuePair<string, string>("data", Query)]);
                using var res = await http.PostAsync(url, content, ct);

                if (res.StatusCode is HttpStatusCode.TooManyRequests or HttpStatusCode.BadGateway
                    or HttpStatusCode.ServiceUnavailable or HttpStatusCode.GatewayTimeout)
                {
                    last = new HttpRequestException($"{url} returned {(int)res.StatusCode}");
                    log.LogWarning("Overpass {Url} busy ({Code}), attempt {N}", url, (int)res.StatusCode, attempt + 1);
                }
                else
                {
                    res.EnsureSuccessStatusCode();
                    await using var stream = await res.Content.ReadAsStreamAsync(ct);
                    return await ParseAsync(stream, ct);
                }
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException && !ct.IsCancellationRequested)
            {
                last = ex;
                log.LogWarning(ex, "Overpass {Url} failed, attempt {N}", url, attempt + 1);
            }

            await Task.Delay(TimeSpan.FromSeconds(5 * Math.Pow(2, attempt)), ct);   // 5s, 10s, 20s, 40s...
        }

        throw new InvalidOperationException("All Overpass servers are busy. Try again in a few minutes.", last);
    }

    private static async Task<IReadOnlyList<OsmElement>> ParseAsync(Stream stream, CancellationToken ct)
    {
        using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: ct);
        var result = new List<OsmElement>();

        foreach (var el in doc.RootElement.GetProperty("elements").EnumerateArray())
        {
            double lat, lon;
            if (el.TryGetProperty("center", out var c))
            { lat = c.GetProperty("lat").GetDouble(); lon = c.GetProperty("lon").GetDouble(); }
            else if (el.TryGetProperty("lat", out var la))
            { lat = la.GetDouble(); lon = el.GetProperty("lon").GetDouble(); }
            else continue;

            var tags = new Dictionary<string, string>();
            if (el.TryGetProperty("tags", out var t))
                foreach (var p in t.EnumerateObject()) tags[p.Name] = p.Value.GetString() ?? "";

            result.Add(new OsmElement(el.GetProperty("type").GetString()!, el.GetProperty("id").GetInt64(), lat, lon, tags));
        }
        return result;
    }
}