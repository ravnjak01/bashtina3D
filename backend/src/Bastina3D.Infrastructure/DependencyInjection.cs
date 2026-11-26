// Infrastructure/DependencyInjection.cs
using Bastina3D.Application.Abstractions;
using Bastina3D.Application.Interfaces;
using Bastina3D.Infrastructure.Data;
using Bastina3D.Infrastructure.Import;
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

        services.AddHttpClient<IOverpassClient, OverpassClient>(c =>
        {
            c.BaseAddress = new Uri("https://overpass.private.coffee/api/");
            c.Timeout = TimeSpan.FromMinutes(3);
            c.DefaultRequestHeaders.UserAgent.ParseAdd("Bastina3D/0.1 (contact: ravnjak01@gmail.com)");
            c.DefaultRequestHeaders.Accept.ParseAdd("application/json");
            // Overpass asks for an identifying UA
        });

        services.AddHttpClient<IWikiClient, WikiClient>(c =>
        {
            c.Timeout = TimeSpan.FromSeconds(30);
            c.DefaultRequestHeaders.UserAgent.ParseAdd("Bastina3D/0.1 (contact: ravnjak01@gmail.com)"); // Wikimedia requires a real UA
        });

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