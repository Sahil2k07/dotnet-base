using DotnetBase.Data.Context;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DotnetBase.Server.IntegrationTest.Infrastructure;

public sealed class IntegrationTestWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _connection;

    public IntegrationTestWebApplicationFactory()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration(
            (_, config) =>
            {
                config.AddInMemoryCollection(
                    new Dictionary<string, string?>
                    {
                        ["Database:Server"] = "localhost",
                        ["Database:Database"] = "integration-test",
                        ["Database:Username"] = "test",
                        ["Database:Password"] = "test",
                    }
                );
            }
        );

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<DotnetBaseContext>>();

            services.AddDbContext<DotnetBaseContext>(options =>
            {
                options.UseSqlite(_connection);
            });
        });
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _connection.Dispose();
        }

        base.Dispose(disposing);
    }
}
