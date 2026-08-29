using DotnetBase.Data.Context;
using DotnetBase.Data.Extension;
using DotnetBase.Data.SQL;
using Microsoft.Extensions.DependencyInjection;

namespace DotnetBase.Data;

public static class DotnetMigrationData
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddDotnetMigrationData()
        {
            #region Options

            services.AddDatabaseOptions();

            #endregion

            services.AddDbContext<DotnetBaseContext>();

            services.AddScoped<ISQLExecutor, SQLExecutor>();

            return services;
        }
    }
}