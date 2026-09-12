using DotnetBase.Service.Auth;
using DotnetBase.Service.User;
using Microsoft.Extensions.DependencyInjection;

namespace DotnetBase.Service;

public static class DotnetBaseService
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddDotnetBaseService()
        {
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IUserService, UserService>();

            return services;
        }
    }
}
