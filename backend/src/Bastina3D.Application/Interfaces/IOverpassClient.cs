using System;
using System.Collections.Generic;
using System.Text;

namespace Bastina3D.Application.Interfaces
{
    public record OsmElement(string Type, long Id, double Lat, double Lon, IReadOnlyDictionary<string, string> Tags);

    public interface IOverpassClient
    {
        Task<IReadOnlyList<OsmElement>> FetchHeritageElementsAsync(CancellationToken ct);
    }
}
