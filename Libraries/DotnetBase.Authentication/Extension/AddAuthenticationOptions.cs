using DotnetBase.Authentication.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DotnetBase.Authentication.Extension;

public static class AuthenticationExtension
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddAuthenticationOptions()
        {
            services
                .AddOptions<AuthenticationOption>()
                .Configure<IConfiguration>(
                    (options, configuration) =>
                    {
                        options.JwtSigningSecret =
                            configuration["Authentication:JwtSigningSecret"]
                            ?? configuration["JWT_SIGNING_SECRET"]
                            ?? throw new InvalidOperationException(
                                "JWT signing secret is required."
                            );

                        options.JwtEncryptionSecret =
                            configuration["Authentication:JwtEncryptionSecret"]
                            ?? configuration["JWT_ENCRYPTION_SECRET"]
                            ?? throw new InvalidOperationException(
                                "JWT encryption secret is required."
                            );

                        options.JwtIssuer =
                            configuration["Authentication:JwtIssuer"]
                            ?? configuration["JWT_ISSUER"]
                            ?? throw new InvalidOperationException("JWT issuer is required.");

                        options.JwtAudience =
                            configuration["Authentication:JwtAudience"]
                            ?? configuration["JWT_AUDIENCE"]
                            ?? throw new InvalidOperationException("JWT audience is required.");

                        options.AccessTokenExpirationMinutes = int.TryParse(
                            configuration["Authentication:AccessTokenExpirationMinutes"]
                                ?? configuration["JWT_ACCESS_TOKEN_EXPIRATION_MINUTES"],
                            out var accessTokenExpiration
                        )
                            ? accessTokenExpiration
                            : throw new InvalidOperationException(
                                "Access token expiration is required."
                            );

                        options.RefreshTokenExpirationDays = int.TryParse(
                            configuration["Authentication:RefreshTokenExpirationDays"]
                                ?? configuration["JWT_REFRESH_TOKEN_EXPIRATION_DAYS"],
                            out var refreshTokenExpiration
                        )
                            ? refreshTokenExpiration
                            : throw new InvalidOperationException(
                                "Refresh token expiration is required."
                            );

                        options.BcryptWorkFactor = int.TryParse(
                            configuration["Authentication:BcryptWorkFactor"]
                                ?? configuration["BCRYPT_WORK_FACTOR"],
                            out var bcryptWorkFactor
                        )
                            ? bcryptWorkFactor
                            : throw new InvalidOperationException(
                                "BCrypt work factor is required."
                            );
                    }
                );

            return services;
        }
    }
}
