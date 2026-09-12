using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using System.Text;
using DotnetBase.Authentication.Authorization.Requirement;
using DotnetBase.Authentication.Configuration;
using DotnetBase.Contract.Common;
using DotnetBase.Shared.Constant;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
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

                foreach (string permission in permissions)
                {
                    options.AddPolicy(
                        permission,
                        policy =>
                        {
                            policy.RequireAuthenticatedUser();
                            policy.AddRequirements(new PermissionRequirement(permission));
                        }
                    );
                }
            });

            services
                .AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
                .Configure<IOptions<AuthenticationOption>>(
                    (options, authenticationOptions) =>
                    {
                        AuthenticationOption authentication = authenticationOptions.Value;

                        var signingKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(authentication.JwtSigningSecret)
                        );

                        var encryptionKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(authentication.JwtEncryptionSecret)
                        );

                        options.MapInboundClaims = false;

                        options.TokenValidationParameters = new TokenValidationParameters
                        {
                            ValidateIssuerSigningKey = true,
                            IssuerSigningKey = signingKey,

                            TokenDecryptionKey = encryptionKey,

                            ValidateIssuer = true,
                            ValidIssuer = authentication.JwtIssuer,

                            ValidateAudience = true,
                            ValidAudience = authentication.JwtAudience,

                            ValidateLifetime = true,

                            ClockSkew = TimeSpan.Zero,

                            NameClaimType = JwtRegisteredClaimNames.Sub,
                        };

                        options.Events = new JwtBearerEvents
                        {
                            OnChallenge = async context =>
                            {
                                context.HandleResponse();

                                context.Response.StatusCode = StatusCodes.Status401Unauthorized;

                                context.Response.ContentType = "application/json";

                                await context.Response.WriteAsJsonAsync(
                                    new ApiResponse<object>
                                    {
                                        Success = false,
                                        Message = "Authentication is required.",
                                        Data = null,
                                    }
                                );
                            },

                            OnForbidden = async context =>
                            {
                                context.Response.StatusCode = StatusCodes.Status403Forbidden;

                                context.Response.ContentType = "application/json";

                                await context.Response.WriteAsJsonAsync(
                                    new ApiResponse<object>
                                    {
                                        Success = false,
                                        Message =
                                            "You do not have permission to access this resource.",
                                        Data = null,
                                    }
                                );
                            },
                        };
                    }
                );

            return services;
        }
    }
}
