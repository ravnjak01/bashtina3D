using System.Text.Json;
using System.Text.RegularExpressions;
using Bastina3D.Application.Interfaces;

namespace Bastina3D.Infrastructure.Import;

public class WikiClient(HttpClient http) : IWikiClient
{
    public async Task<IReadOnlyList<WikidataEntity>> GetEntitiesAsync(IReadOnlyCollection<string> ids, CancellationToken ct)
    {
        var url = "https://www.wikidata.org/w/api.php?action=wbgetentities&format=json" +
                  "&props=labels|claims|sitelinks&languages=en|bs&sitefilter=enwiki|bswiki" +
                  $"&ids={string.Join('|', ids)}";
        using var doc = await GetJsonAsync(url, ct);
        var list = new List<WikidataEntity>();
        if (!doc.RootElement.TryGetProperty("entities", out var ents)) return list;

        foreach (var ent in ents.EnumerateObject())
        {
            var v = ent.Value;
            if (v.TryGetProperty("missing", out _)) continue;

            string? Label(string l) => v.TryGetProperty("labels", out var ls) && ls.TryGetProperty(l, out var x)
                ? x.GetProperty("value").GetString() : null;
            string? Site(string k) => v.TryGetProperty("sitelinks", out var sl) && sl.TryGetProperty(k, out var x)
                ? x.GetProperty("title").GetString() : null;

            string? image = null; int? year = null;
            if (v.TryGetProperty("claims", out var claims))
            {
                if (claims.TryGetProperty("P18", out var p18) && p18.GetArrayLength() > 0 &&
                    p18[0].GetProperty("mainsnak").TryGetProperty("datavalue", out var dv))
                    image = dv.GetProperty("value").GetString();

                if (claims.TryGetProperty("P571", out var p571) && p571.GetArrayLength() > 0 &&
                    p571[0].GetProperty("mainsnak").TryGetProperty("datavalue", out var tv))
                {
                    var m = Regex.Match(tv.GetProperty("value").GetProperty("time").GetString() ?? "", @"^\+(\d{4})");
                    if (m.Success) year = int.Parse(m.Groups[1].Value);
                }
            }
            list.Add(new WikidataEntity(ent.Name, Label("en"), Label("bs"), image, year, Site("enwiki"), Site("bswiki")));
        }
        await Task.Delay(300, ct);                                   
        return list;
    }

    public async Task<string?> GetSummaryAsync(string lang, string title, CancellationToken ct)
    {
        var url = $"https://{lang}.wikipedia.org/api/rest_v1/page/summary/{Uri.EscapeDataString(title.Replace(' ', '_'))}";
        try
        {
            using var doc = await GetJsonAsync(url, ct);
            await Task.Delay(200, ct);
            return doc.RootElement.TryGetProperty("extract", out var x) ? x.GetString() : null;
        }
        catch (HttpRequestException) { return null; }                // 404 etc: skip this page
    }

    public async Task<ImageInfo> GetImageInfoAsync(string fileName, CancellationToken ct)
    {
        var url = "https://commons.wikimedia.org/w/api.php?action=query&prop=imageinfo&iiprop=extmetadata&format=json" +
                  $"&titles={Uri.EscapeDataString("File:" + fileName)}";
        try
        {
            using var doc = await GetJsonAsync(url, ct);
            await Task.Delay(200, ct);
            foreach (var page in doc.RootElement.GetProperty("query").GetProperty("pages").EnumerateObject())
            {
                if (!page.Value.TryGetProperty("imageinfo", out var ii) || ii.GetArrayLength() == 0) continue;
                var meta = ii[0].GetProperty("extmetadata");
                string? Val(string k) => meta.TryGetProperty(k, out var x) ? StripHtml(x.GetProperty("value").GetString()) : null;
                return new ImageInfo(Val("Artist"), Val("LicenseShortName"));
            }
        }
        catch (HttpRequestException) { }
        return new ImageInfo(null, null);
    }

    private async Task<JsonDocument> GetJsonAsync(string url, CancellationToken ct)
    {
        using var res = await http.GetAsync(url, ct);
        res.EnsureSuccessStatusCode();
        return await JsonDocument.ParseAsync(await res.Content.ReadAsStreamAsync(ct), cancellationToken: ct);
    }

    private static string? StripHtml(string? s) =>
        s is null ? null : System.Net.WebUtility.HtmlDecode(Regex.Replace(s, "<.*?>", "")).Trim() is { Length: > 0 } t
            ? (t.Length > 300 ? t[..300] : t) : null;
}