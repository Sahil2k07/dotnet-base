using DotnetBase.Data.Context;
using DotnetBase.Data.Extension;
using DotnetBase.Data.Repository;
using DotnetBase.Data.Repository.Implementation;
using DotnetBase.Data.SQL;
using Microsoft.Extensions.DependencyInjection;

namespace DotnetBase.Data;

public static class DotnetBaseData
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddDotnetBaseData()
        {
            #region Options

            services.AddDatabaseOptions();

            #endregion

            services.AddDbContext<DotnetBaseContext>();

            services.AddScoped<ISQLExecutor, SQLExecutor>();

            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IUserRoleRepository, UserRoleRepository>();
            services.AddScoped<IUserSessionRepository, UserSessionRepository>();

            services.AddScoped<IRoleRepository, RoleRepository>();

            return services;
        }
    }
}
