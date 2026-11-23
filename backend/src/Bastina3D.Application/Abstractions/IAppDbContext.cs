using Bastina3D.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Bastina3D.Application.Abstractions
{
    public interface IAppDbContext
    {
        DbSet<HeritageSite> Sites { get; }
        DbSet<SiteModel3D> Models { get; }
        DbSet<SiteMedia> Media { get; }
        Task<int> SaveChangesAsync(CancellationToken ct = default);
    }
}
