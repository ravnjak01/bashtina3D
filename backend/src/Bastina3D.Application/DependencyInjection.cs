using Bastina3D.Application.Import;
using Bastina3D.Application.Interfaces;
using Bastina3D.Application.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bastina3D.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddScoped<ISiteService, SiteService>();
            services.AddScoped<IHeritageImporter, OsmImporter>();
            services.AddScoped<IWikidataEnricher, WikidataEnricher>();

            return services;
        }
    }
}
