using Bastina3D.Domain.Enums;
using NetTopologySuite.Geometries;
namespace Bastina3D.Domain.Entities
{
    public class HeritageSite
    {
        public Guid Id { get; set; } = Guid.NewGuid();   // client-generated: identical behavior on SQL Server & Postgres
        public string Name { get; set; } = default!;
        public string? NameLocal { get; set; }
        public string Slug { get; set; } = default!;
        public string? Description { get; set; }
        public SiteCategory Category { get; set; }
        public string? Period { get; set; }               // "Ottoman", "Medieval", ...
        public int? YearBuilt { get; set; }
        public string? Municipality { get; set; }
        public bool IsNationalMonument { get; set; }

        public Point Location { get; set; } = default!;   // SRID 4326, Point(lon, lat)
        public double? ElevationM { get; set; }

        // Import provenance (for idempotent re-imports)
        public string? WikidataId { get; set; }
        public string? OsmType { get; set; }              // node | way | relation
        public long? OsmId { get; set; }

        public string? HeroImageUrl { get; set; }
        public SiteModel3D? Model { get; set; }
        public List<SiteMedia> Media { get; set; } = new();

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
    }
}
