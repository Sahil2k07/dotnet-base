using DotnetBase.Authentication.Extension;
using DotnetBase.Authentication.Service;
using DotnetBase.Authentication.Service.Implementation;
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

            services.AddSingleton<ICryptoService, CryptoService>();
            services.AddScoped<ICurrentUser, CurrentUser>();

            services.AddJwtAuthentication();

            return services;
        }
    }
}
