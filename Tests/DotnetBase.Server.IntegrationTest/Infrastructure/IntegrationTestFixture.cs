using DotnetBase.Data.Context;
using DotnetBase.Server.IntegrationTest.Infrastructure.Seed;
using Microsoft.Extensions.DependencyInjection;

namespace DotnetBase.Server.IntegrationTest.Infrastructure;

public sealed class IntegrationTestFixture : IAsyncLifetime
{
    public IntegrationTestWebApplicationFactory Factory { get; } = new();

    public HttpClient Client { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        Client = Factory.CreateClient();

        using var scope = Factory.Services.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<DotnetBaseContext>();

        await context.Database.EnsureCreatedAsync();

        await RoleSeed.SeedRoles(context);
        await PermissionSeed.SeedPermissions(context);
        await RolePermissionSeed.SeedAsync(context);
    }

    public async Task DisposeAsync()
    {
        Client.Dispose();

        await Factory.DisposeAsync();
    }
}
