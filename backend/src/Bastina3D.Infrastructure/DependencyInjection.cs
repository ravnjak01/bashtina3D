// Infrastructure/DependencyInjection.cs
using Bastina3D.Application.Abstractions;
using Bastina3D.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Bastina3D.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<AppDbContext>(opt =>
        {
            var cs = config.GetConnectionString("Default");
            switch (config["Database:Provider"] ?? "SqlServer")
            {
                case "SqlServer": opt.UseSqlServer(cs, o => o.UseNetTopologySuite()); break;
                // case "Postgres": opt.UseNpgsql(cs, o => o.UseNetTopologySuite()); break;
                default: throw new InvalidOperationException("Unknown Database:Provider");
            }
        });

        services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());
        return services;
    }

    public static async Task InitializeDatabaseAsync(this IServiceProvider sp)
    {
        using var scope = sp.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();
        await DbSeeder.SeedAsync(db);
    }
}