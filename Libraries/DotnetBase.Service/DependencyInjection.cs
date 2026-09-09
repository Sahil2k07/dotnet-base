using DotnetBase.Service.Auth;
using Microsoft.Extensions.DependencyInjection;

namespace DotnetBase.Service;

public static class DotnetBaseService
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddDotnetBaseService()
        {
            services.AddScoped<IAuthService, AuthService>();

            return services;
        }
    }
}
