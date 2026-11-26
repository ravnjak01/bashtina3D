using System.Text.RegularExpressions;
using Bastina3D.Domain.Enums;

namespace Bastina3D.Application.Import;

public static class OsmTagMapper
{
    public static SiteCategory MapCategory(IReadOnlyDictionary<string, string> t)
    {
        string G(string k) => t.TryGetValue(k, out var v) ? v : "";
        var historic = G("historic"); var building = G("building");
        var amenity = G("amenity"); var religion = G("religion");

        if (building == "mosque" || (amenity == "place_of_worship" && religion == "muslim")) return SiteCategory.Mosque;
        if (amenity == "monastery" || historic == "monastery") return SiteCategory.Monastery;
        if (building is "church" or "cathedral" or "chapel" || (amenity == "place_of_worship" && religion == "christian"))
            return SiteCategory.Church;

        return historic switch
        {
            "castle" or "fort" or "city_gate" or "citywalls" or "tower" => SiteCategory.Fortress,
            "archaeological_site" or "ruins" => SiteCategory.Archaeological,
            "tomb" or "cemetery" or "grave_yard" => SiteCategory.Necropolis,
            "monument" or "memorial" or "wayside_cross" or "wayside_shrine" => SiteCategory.Monument,
            "bridge" => SiteCategory.Bridge,
            _ when G("man_made") == "bridge" => SiteCategory.Bridge,
            _ when G("tourism") == "museum" => SiteCategory.Museum,
            _ => SiteCategory.Other,
        };
    }

    // name:en for the display name, name:bs (or plain name) for the local one
    public static string? GetName(IReadOnlyDictionary<string, string> t) =>
        First(t, "name:en", "name", "name:bs", "name:sr", "name:hr");

    public static string? GetLocalName(IReadOnlyDictionary<string, string> t) =>
        First(t, "name:bs", "name", "name:hr", "name:sr");

    public static string? GetMunicipality(IReadOnlyDictionary<string, string> t) =>
        First(t, "addr:municipality", "addr:city");

    public static int? ParseYear(IReadOnlyDictionary<string, string> t)
    {
        if (!t.TryGetValue("start_date", out var s)) return null;
        var m = Regex.Match(s, @"\b(\d{4})\b");
        return m.Success && int.Parse(m.Groups[1].Value) is >= 1 and <= 2100 ? int.Parse(m.Groups[1].Value) : null;
    }

    // Rough buckets only, refine by hand for important sites.
    public static string? PeriodFromYear(int? y) => y switch
    {
        null => null,
        < 1463 => "Medieval",
        < 1878 => "Ottoman",
        < 1918 => "Austro-Hungarian",
        _ => "Modern",
    };

    private static string? First(IReadOnlyDictionary<string, string> t, params string[] keys)
    {
        foreach (var k in keys)
            if (t.TryGetValue(k, out var v) && !string.IsNullOrWhiteSpace(v)) return v.Trim();
        return null;
    }
}