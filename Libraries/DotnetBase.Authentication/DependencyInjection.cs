using DotnetBase.Authentication.Authorization.Handler;
using DotnetBase.Authentication.Extension;
using DotnetBase.Authentication.Service;
using DotnetBase.Authentication.Service.Implementation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace DotnetBase.Authentication;

public static class DotnetBaseAuthentication
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddDotnetBaseAuthentication()
        {
            services.AddHttpContextAccessor();

            services.AddAuthenticationOptions();

            services.AddScoped<IAuthorizationHandler, RoleAuthorizationHandler>();
            services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

            services.AddSingleton<ICryptoService, CryptoService>();
            services.AddScoped<ICurrentUser, CurrentUser>();

            services.AddJwtAuthentication();

            return services;
        }
    }
}
