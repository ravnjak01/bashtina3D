using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bastina3D.Application.Abstractions
{
    public interface IGeometryFactoryService
    {
        Point CreateWgs84Point(double lon, double lat);
    }
}
