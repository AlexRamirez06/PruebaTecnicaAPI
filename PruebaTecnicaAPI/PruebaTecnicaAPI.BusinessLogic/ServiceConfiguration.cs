using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using PruebaTecnicaAPI.BusinessLogic.Services;
using PruebaTecnicaAPI.DataAccess;
using PruebaTecnicaAPI.DataAccess.Repositories;

namespace PruebaTecnicaAPI.BusinessLogic
{
   public static class ServiceConfiguration
    {
        public static void DataAccess(this IServiceCollection services, string connectionString)
        {
            services.AddScoped<ClienteRepository>();
            services.AddScoped<ProductoRepository>();
            services.AddScoped<OrdenRepository>();


            PruebaTecnicaAPIContext.BuildConnectionString(connectionString);
        }

        public static void BusinessLogic(this IServiceCollection services)
        {
            services.AddScoped<GeneralService>();
            //services.AddScoped<AccesoService>();

        }
    }
}
