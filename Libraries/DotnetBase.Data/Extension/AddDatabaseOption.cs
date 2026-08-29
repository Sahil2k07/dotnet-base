using DotnetBase.Data.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DotnetBase.Data.Extension;

public static class DatabaseExtension
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddDatabaseOptions()
        {
            services
                .AddOptions<DatabaseOption>()
                .Configure<IConfiguration>(
                    (options, configuration) =>
                    {
                        options.Server =
                            configuration["Database:Server"]
                            ?? configuration["DB_SERVER"]
                            ?? throw new InvalidOperationException("Database server is required.");

                        options.Database =
                            configuration["Database:Database"]
                            ?? configuration["DB_DATABASE"]
                            ?? throw new InvalidOperationException(
                                "Database database is required."
                            );

                        options.Username =
                            configuration["Database:Username"]
                            ?? configuration["DB_USERNAME"]
                            ?? throw new InvalidOperationException(
                                "Database username is required."
                            );

                        options.Password =
                            configuration["Database:Password"]
                            ?? configuration["DB_PASSWORD"]
                            ?? throw new InvalidOperationException(
                                "Database password is required."
                            );
                    }
                );

            return services;
        }
    }
}