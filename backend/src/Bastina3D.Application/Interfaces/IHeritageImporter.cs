using System;
using System.Collections.Generic;
using System.Text;

namespace Bastina3D.Application.Interfaces
{
    public record ImportResult(int Fetched, int Created, int Updated, int Skipped);

    public interface IHeritageImporter
    {
        Task<ImportResult> ImportFromOsmAsync(CancellationToken ct);
    }
}
