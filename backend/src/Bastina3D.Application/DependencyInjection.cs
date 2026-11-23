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
            return services;
        }
    }
}
