using NetTopologySuite;
using NetTopologySuite.Geometries;

namespace Bastina3D.Infrastructure;

public static class Geo
{
    public const int Wgs84 = 4326;
    private static readonly GeometryFactory Factory =
        NtsGeometryServices.Instance.CreateGeometryFactory(Wgs84);

    // Coordinate order is (X = lon, Y = lat). Easy to get backwards.
    public static Point CreatePoint(double lon, double lat) => Factory.CreatePoint(new Coordinate(lon, lat));
}
