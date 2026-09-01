using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using System.Text;
using DotnetBase.Authentication.Configuration;
using DotnetBase.Shared.Constant;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace DotnetBase.Authentication.Extension;

public static class JwtAuthenticationExtension
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddJwtAuthentication()
        {
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();

            services.AddAuthorization(options =>
            {
                var permissions = typeof(Permissions)
                    .GetFields(BindingFlags.Public | BindingFlags.Static)
                    .Where(field => field.IsLiteral && !field.IsInitOnly)
                    .Where(field => field.FieldType == typeof(string))
                    .Select(field => (string)field.GetRawConstantValue()!);

                foreach (var permission in permissions)
                {
                    options.AddPolicy(
                        permission,
                        policy => policy.RequireClaim("permissions", permission)
                    );
                }
            });

            services
                .AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
                .Configure<AuthenticationOption>(
                    (options, authenticationOptions) =>
                    {
                        var signingKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(authenticationOptions.JwtSigningSecret)
                        );

                        var encryptionKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(authenticationOptions.JwtEncryptionSecret)
                        );

                        options.MapInboundClaims = false;

                        options.TokenValidationParameters = new TokenValidationParameters
                        {
                            ValidateIssuerSigningKey = true,
                            IssuerSigningKey = signingKey,

                            TokenDecryptionKey = encryptionKey,

                            ValidateIssuer = true,
                            ValidIssuer = authenticationOptions.JwtIssuer,

                            ValidateAudience = true,
                            ValidAudience = authenticationOptions.JwtAudience,

                            ValidateLifetime = true,

                            ClockSkew = TimeSpan.Zero,

                            RoleClaimType = "active_role",
                            NameClaimType = JwtRegisteredClaimNames.Sub,
                        };
                    }
                );

            return services;
        }
    }
}
