using Bastina3D.Domain.Entities;
using Bastina3D.Domain.Enums;

namespace Bastina3D.Application.Import;

public static class RankCalculator
{
    public static int Compute(HeritageSite s)
    {
        var score = 0;
        if (s.IsNationalMonument) score += 50;
        if (s.WikidataId != null) score += 30;   // notable enough for Wikidata
        if (!string.IsNullOrEmpty(s.Description)) score += 10;
        if (s.HeroImageUrl != null) score += 10;
        if (s.Model != null) score += 40;   // you built a 3D model, so feature it
        if (s.YearBuilt != null) score += 5;
        score += s.Category switch
        {
            SiteCategory.Fortress or SiteCategory.OldTown or SiteCategory.Bridge => 10,
            SiteCategory.Mosque or SiteCategory.Church or SiteCategory.Monastery => 5,
            SiteCategory.Other => -10,
            _ => 0
        };
        return score;
    }
}